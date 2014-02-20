using UnityEngine;
using System.Collections;

public class TailGenerator : MonoBehaviour {

    //
    // "Constuct"
    //


    public void inject (Transform rootAnchor, float maxVirtualVelocity) {
        _rootAnchor = rootAnchor;
        _maxVirtualVelocity = maxVirtualVelocity;
    }

    //
    // Public scope
    //

    public Transform tailPiecePrefab;


    //
    // Private scope
    //

    private LinkLogic _rootLink;
    private Transform _rootAnchor;



    private float _maxVirtualVelocity;
    private Vector3 _currentVirtualVel;

    public void Start () {

    }

    public void FixedUpdate () {
        if (_rootLink == null)
            return;

        _rootLink.externalUpdate (_maxVirtualVelocity, _currentVirtualVel, Time.fixedDeltaTime);
    }

    //
    // API
    //

    public void updateInfo (Vector3 virtualVel) {
        _currentVirtualVel = virtualVel;
        //Debug.Log (this._anchor.position);
    }


    public void addLink () {
        if (_rootLink == null) {
            Transform tr = (Transform)Instantiate (tailPiecePrefab, _rootAnchor.position, Quaternion.identity);
            _rootLink = tr.GetComponent<LinkLogic> ();
            _rootLink.setAnchor (_rootAnchor);
        } else {
            _rootLink.addLink (tailPiecePrefab);
        }
    }
}