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

    public Transform TailPiecePrefab;


    //
    // Private scope
    //

    private LinkLogic _rootLink;
    private Transform _rootAnchor;


    private float _maxVirtualVelocity;
    private Vector3 _currentVirtualVel;


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
            Transform tr = (Transform)Instantiate (TailPiecePrefab, _rootAnchor.position, Quaternion.identity);
            _rootLink = tr.GetComponent<LinkLogic> ();
            _rootLink.Inject (_rootAnchor);
        } else {
            _rootLink.addLink ();
        }
    }
}