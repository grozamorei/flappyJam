using UnityEngine;
using System.Collections;

public class TailLogic : MonoBehaviour {

	//
	// Public scope
	//

	public Transform linkPrefab;
	public float lerp = 0.1f;

	//
	// Private scope
	//

	private Transform _anchor;
	private TailLink _rootLink;

	//
	// System methods
	//
	public void Start () {
	
	}
	
	public void FixedUpdate () {
		if (_rootLink != null) {
			_rootLink.update(lerp);
		}
	}

	//
	// API
	//

	public void injectFirstAnchor(Transform anchor) {
		this._anchor = anchor;
	}

	public void updatePosition() {
		Debug.Log(this._anchor.position);
	}

	public void addLink() {
		if (_rootLink == null) {
			_rootLink = new TailLink(
				(Transform) Instantiate(linkPrefab, Vector3.zero, Quaternion.identity), 
				this._anchor
			);
		} else {
			//stub
		}
	}
}

class TailLink {

	public TailLink(Transform linkDisplay, Transform anchor) {
		_link = linkDisplay;
		_link.position = new Vector3(anchor.position.x, anchor.position.y, anchor.position.z);
		this._anchor = anchor;
	}

	private Transform _link;
	private Transform _anchor;

	public void update(float withLerp) {
		Vector3 self = _link.position;
		Vector3 anchor = _anchor.position;

		_link.position = new Vector3(
			Mathf.Lerp(self.x, anchor.x, withLerp),
			Mathf.Lerp(self.y, anchor.y, withLerp),
			Mathf.Lerp(self.z, anchor.z, withLerp)
		);
	}
}