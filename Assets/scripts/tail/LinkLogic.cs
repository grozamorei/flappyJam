using UnityEngine;
using System.Collections;

public class LinkLogic : MonoBehaviour {

    //
    // Public scope
    //

    public LinkParams[] links;

    [Range (0.05f, 1f)]
    public float
        minVelocityLerp = 0.9f;
    [Range (0.05f, 1f)]
    public float
        maxVelocityLerp = 0.5f;

    //
    // Private scope
    //

    private Transform _parentAnchor;
    private Transform _selfAnchor;

    private LinkLogic _parent;
    private LinkLogic _child;

    public void setAnchor (Transform parentAnchor) {
        this._parentAnchor = parentAnchor;
    }

    void Start () {
        //Debug.Log ("SCALE: " + links [0]);

        int myDepth = detectDepth ();
        transform.localScale = new Vector3 (links [myDepth].scale, links [myDepth].scale, links [myDepth].scale);

        foreach (Transform child in this.transform) {
            if (child.name == "tail anchor") {
                _selfAnchor = child;
            }
        }
    }

    //
    // API
    //

    public void addLink (Transform prefab) {
        if (_child == null) {
            //_child = (LinkLogic)Instantiate (this.gameObject, _selfAnchor.position, Quaternion.identity);
            //_child._parent = this;
            Transform tr = (Transform)Instantiate (prefab, _selfAnchor.position, Quaternion.identity);
            _child = tr.GetComponent<LinkLogic> ();
            _child.setAnchor (_selfAnchor);
            _child._parent = this;
        } else {
            _child.addLink (prefab);
        }
    }
    
    
    public void externalUpdate (float maxVelXZ, 
                        Vector3 currentVel, float timeDelta) {
        int myDepth = detectDepth ();
        float minBound = this.links [myDepth].minBound;

        Vector2 currentVelXZ = new Vector2 (currentVel.x, currentVel.z);
        Vector2 selfPosXZ = new Vector2 (this.transform.position.x, this.transform.position.z);
        Vector2 anchorPosXZ = new Vector2 (_parentAnchor.position.x, _parentAnchor.position.z);
        
        float distance = Vector2.Distance (selfPosXZ, anchorPosXZ);
        //Debug.Log (distance + "; " + myDepth + "; " + minBound);
        
        float maxTravelDistance = distance - minBound - 0.01f;
        Vector2 middlePosXZ;
        
        if (distance >= minBound) {
            
            float velocityRatio = currentVelXZ.magnitude / maxVelXZ;
            float lerpDiff = minVelocityLerp - maxVelocityLerp;
            
            float currentLerp = minVelocityLerp - lerpDiff * velocityRatio;
            
            //Debug.Log (velocityRatio + "; " + currentVelXZ);
            
            //Debug.Log (dist + " " + maxBound);
            middlePosXZ = Vector2.MoveTowards (selfPosXZ, anchorPosXZ, maxTravelDistance * currentLerp);
            this.transform.position = new Vector3 (middlePosXZ.x, this.transform.position.y, middlePosXZ.y);
        }
        
        //return;
        
        if (_child == null)
            return;
        
        _child.externalUpdate (maxVelXZ, currentVel, timeDelta);
    }

    //
    // Self API
    //
    private int detectDepth () {
        if (_parent != null)
            return _parent.detectDepth () + 1;
        return 0;
    }
}
