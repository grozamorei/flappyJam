using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Camera mainCamera;
    public float lerp = 0.1f;
    public float yLerp = 0.1f;
    public float yFollowThreshold = 1f;

    private Vector3 _startDiff;

    private float _groundPoint;

	// Use this for initialization
	void Start () {
        Vector3 self = this.transform.position;
        Vector3 camera = this.mainCamera.transform.position;

        _groundPoint = self.y;
        _startDiff = new Vector3(self.x - camera.x, self.y - camera.y, self.z - camera.z);

        Debug.Log(_startDiff);
	}

    public void updateGroundPoint(float currentY) {
        if(Mathf.Abs(currentY - _groundPoint) > yFollowThreshold) {
            //Debug.Log("Updated! from: " + _groundPoint + " to:" + currentY);
            _groundPoint = currentY;
        }
    }

    private void FixedUpdate() {
        Vector3 self = this.transform.position;
        Vector3 camera = this.mainCamera.transform.position;

        Vector3 currentDiff = new Vector3(self.x - camera.x - _startDiff.x, _groundPoint - camera.y - _startDiff.y, self.z - camera.z - _startDiff.z);
        //Debug.Log(currentDiff.y);

        if (currentDiff.magnitude > 0) {
            this.mainCamera.transform.position = new Vector3(
                Mathf.Lerp(camera.x, camera.x + currentDiff.x, this.lerp),
                Mathf.Lerp(camera.y, _groundPoint - _startDiff.y, this.yLerp),
                Mathf.Lerp(camera.z, camera.z + currentDiff.z, this.lerp)
            );
            //Mathf.Lerp(camera.y, camera.y + currentDiff.y
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
