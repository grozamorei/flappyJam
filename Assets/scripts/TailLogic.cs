using UnityEngine;
using System.Collections;

public class TailLogic : MonoBehaviour
{

    //
    // Public scope
    //

    public Transform linkPrefab;

    public float maxMagnitude = 1f;

    public float lerp = 0.1f;
    public float angleLerp = 0.05f;

    //
    // Private scope
    //

    private Transform _anchor;
    private Transform _head;

    private TailLink _rootLink;
    private bool _headMoving = false;

    //
    // System methods
    //
    public void Start ()
    {
	
    }
	
    public void FixedUpdate ()
    {
        //if (!_headMoving)
        //		return;
        if (_rootLink == null)
            return;
        Vector3 vel = _head.GetComponent<Rigidbody> ().velocity;
        //Debug.Log (vel.magnitude);
        //if (Mathf.Abs (vel.magnitude) > 0.3f) {
        _rootLink.update (lerp, angleLerp, _head.rotation.eulerAngles);

        //}
    }

    //
    // API
    //

    public void injectFirstAnchor (Transform anchor, Transform head)
    {
        _anchor = anchor;
        _head = head;
    }

    public void updateInfo (bool moving)
    {
        _headMoving = moving;
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


    public void update (float withLerp, float withAngleLerp, Vector3 parentEuler)
    {
        Vector3 self = _link.position;
        Vector3 selfEuler = _link.rotation.eulerAngles;

        Vector3 anchor = _parentAnchor.position;

        float dist = Vector3.Distance (self, anchor);
        //Debug.Log (Vector3.Distance (self, anchor));
				
        if (dist > 1f) {
            //Debug.Log (Vector3. (self, anchor));
            Vector3 some = Vector3.MoveTowards (self, anchor, );
            //Debug.Log (some);
            return;
            _link.position = new Vector3 (
                self.x + some.x,
                self.y + some.y,
                self.z + some.z
            );
        }
				
        return;
        _link.position = new Vector3 (
					Mathf.Lerp (self.x, anchor.x, withLerp),
					Mathf.Lerp (self.y, anchor.y, withLerp),
					Mathf.Lerp (self.z, anchor.z, withLerp)
        );

        //_link.rotation = Quaternion.identity;
        //_link.rotation = Quaternion.Euler (
        //	Mathf.LerpAngle (selfEuler.x, parentEuler.x, withAngleLerp),
        //	Mathf.LerpAngle (selfEuler.y, parentEuler.y, withAngleLerp),
        //	Mathf.LerpAngle (selfEuler.z, parentEuler.z, withAngleLerp)
        //);
				
        if (_child == null)
            return;
				
        _child.update (withLerp, withAngleLerp, _link.rotation.eulerAngles);
    }
}