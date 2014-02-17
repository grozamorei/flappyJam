using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	//
	// Public scope
	//
    public Camera mainCamera;
    public float lerp = 0.1f;
    public float yLerp = 0.1f;
    public float yFollowThreshold = 1f;


	//
	// Private scope
	//
	private Vector3 _startDiff;
    private float _groundPoint;


	//
	// System methods
	//

	void Start () {
        Vector3 snakeHead = this.transform.position;
        Vector3 camera = this.mainCamera.transform.position;

        _groundPoint = snakeHead.y;
        _startDiff = new Vector3(snakeHead.x - camera.x, snakeHead.y - camera.y, snakeHead.z - camera.z);


        Debug.Log(_startDiff);
	}

    private void FixedUpdate() {
        Vector3 snakeHead = this.transform.position;
        Vector3 camera = this.mainCamera.transform.position;

        Vector3 currentDiff = new Vector3(
			snakeHead.x - camera.x - _startDiff.x, 
			_groundPoint - camera.y - _startDiff.y, 
			snakeHead.z - camera.z - _startDiff.z
		);

        if (currentDiff.magnitude > 0) {
            this.mainCamera.transform.position = new Vector3(
                Mathf.Lerp(camera.x, camera.x + currentDiff.x, this.lerp),
                Mathf.Lerp(camera.y, _groundPoint - _startDiff.y, this.yLerp),
                Mathf.Lerp(camera.z, camera.z + currentDiff.z, this.lerp)
            );
        }
    }

	//
	// API
	//

	public void updateGroundPoint(float currentY) {
		if(Mathf.Abs(currentY - _groundPoint) > yFollowThreshold) {
			//Debug.Log("Updated! from: " + _groundPoint + " to:" + currentY);
			_groundPoint = currentY;
		}
	}
}
