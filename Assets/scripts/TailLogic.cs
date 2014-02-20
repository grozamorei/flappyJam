using UnityEngine;
using System.Collections;

public class TailLogic : MonoBehaviour
{

    //
    // Public scope
    //

    public Transform linkPrefab;

    public float minBound = 0.6f;
    public float maxBound = 0.8f;
    
    public float minBoundLerp = 0.9f;
    public float maxBoundLerp = 0.5f;


    //
    // Private scope
    //

    private Transform _anchor;
    private Transform _head;
    private float _headMaxVelocity;

    private TailLink _rootLink;
    private bool _headMoving = false;
    private Vector3 _virtualVel;


    //
    // System methods
    //
    public void Start ()
    {
	
    }
	
    public void FixedUpdate ()
    {
        if (_rootLink == null)
            return;

        //Vector3 vel = _head.GetComponent<Rigidbody> ().velocity;

        _rootLink.update (_headMaxVelocity, minBound, maxBound, minBoundLerp, maxBoundLerp, _virtualVel, Time.fixedDeltaTime);
    }

    //
    // API
    //

    public void injectFirstAnchor (Transform anchor, Transform head)
    {
        _anchor = anchor;
        _head = head;
        _headMaxVelocity = head.GetComponent<SnakeController> ().maxSpeed;
    }

    public void updateInfo (bool moving, Vector3 virtualVel)
    {
        _headMoving = moving;
        _virtualVel = virtualVel;
        //Debug.Log (this._anchor.position);
    }

    public void addLink ()
    {
        if (_rootLink == null) {
            _rootLink = new TailLink (
							(Transform)Instantiate (linkPrefab, Vector3.zero, Quaternion.identity), 
							_anchor,
							1
            );
        } else {
            _rootLink.addLink ((Transform)Instantiate (linkPrefab));
        }
    }
}

class TailLink
{

    public TailLink (Transform linkDisplay, Transform anchor, int len)
    {
        _link = linkDisplay;
        _link.position = new Vector3 (anchor.position.x, anchor.position.y, anchor.position.z);
        this._parentAnchor = anchor;
		
        foreach (Transform child in _link) {
            if (child.name == "tail anchor") {
                _selfAnchor = child;
            }
        }
		
        _len = len;
    }

    private Transform _link;
    private Transform _parentAnchor;
    private Transform _selfAnchor;
		
    private TailLink _child;
	
    private int _len = 0;

    public int len {
        get { return _len; } 
    }
	
    public void addLink (Transform linkDisplay)
    {
        _len++;
        if (_child == null) {
            _child = new TailLink (linkDisplay, _selfAnchor, _len);
        } else {
            _child.addLink (linkDisplay);
        }
    }


    public void update (float maxVelXZ, 
                        float minBound, float maxBound, 
                        float minBoundLerp, float maxBoundLerp, 
                        Vector3 currentVel, float timeDelta)
    {
        Vector2 currentVelXZ = new Vector2 (currentVel.x, currentVel.z);
        Vector2 selfPosXZ = new Vector2 (_link.position.x, _link.position.z);
        Vector2 anchorPosXZ = new Vector2 (_parentAnchor.position.x, _parentAnchor.position.z);

        float distance = Vector2.Distance (selfPosXZ, anchorPosXZ);
        //Debug.Log(distance);

        float maxTravelDistance = distance - minBound - 0.01f;
        Vector2 middlePosXZ;

        if (distance >= minBound) {

            float velocityRatio = currentVelXZ.magnitude / maxVelXZ;
            float lerpDiff = minBoundLerp - maxBoundLerp;

            float currentLerp = minBoundLerp - lerpDiff * velocityRatio;

            if (distance >= maxBound) {
                //currentLerp *= 1.1f;
            }
            //Debug.Log (velocityRatio + "; " + currentVelXZ);

            //Debug.Log (dist + " " + maxBound);
            middlePosXZ = Vector2.MoveTowards (selfPosXZ, anchorPosXZ, maxTravelDistance * currentLerp);
            _link.position = new Vector3 (middlePosXZ.x, _link.position.y, middlePosXZ.y);
        }
				
        //return;
				
        if (_child == null)
            return;
				
        _child.update (maxVelXZ, minBound, maxBound, minBoundLerp, maxBoundLerp, currentVel, timeDelta);
    }
}