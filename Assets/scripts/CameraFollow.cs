using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Camera camera;
    public float lerp = 0.8f;

    private Vector3 _startDiff;

	// Use this for initialization
	void Start () {
        Vector3 self = this.transform.position;
        Vector3 camera = this.camera.transform.position;

        _startDiff = new Vector3(self.x - camera.x, self.y - camera.y, self.z - camera.z);
        //Debug.Log(_startDiff);
	}

    private void FixedUpdate() {
        Vector3 self = this.transform.position;
        Vector3 camera = this.camera.transform.position;

        Vector3 currentDiff = new Vector3(self.x - camera.x - _startDiff.x, self.y - camera.y - _startDiff.y, self.z - camera.z - _startDiff.z);
        //Debug.Log(currentDiff);

        if (currentDiff.magnitude > 0) {
            this.camera.transform.position = new Vector3(
                Mathf.Lerp(camera.x, camera.x + currentDiff.x, this.lerp),
                Mathf.Lerp(camera.y, camera.y + currentDiff.y, this.lerp),
                Mathf.Lerp(camera.z, camera.z + currentDiff.z, this.lerp)
            );
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
