using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CameraFollow))]
[RequireComponent(typeof(TailLogic))]
public class SnakeController : MonoBehaviour
{


		//
		// Public scope
		//

		public float maxSpeed = 4f;
		public float jumpImpulse = 12f;

		public float lerpAtMaxSpeed = 0.07f;
		public float lerpAtMinSpeed = 0.2f;
		public float lerpInAir = 0.3f;

		public Camera mainCamera;
		public Transform shadow;
    
		public LayerMask groundLayer;
		public LayerMask wallLayer;
		public LayerMask deathTriggerLayer;
		public LayerMask shadowReflectionLayer;


		//
		// Private scope
		//

		private bool isGrounded = false;
    
		private CameraFollow _followScript;
		private TailLogic _tailScript;
		private Transform _tailAnchor;

		private Animator _animator;

		private float minimumDistance;
		private float shadowOriginalScale;

		private float _cameraDiff;
		private Dictionary<float, Dictionary<float, float>> _keyToRotation;

		private Vector2 _rawHV = Vector2.zero;
		private Vector2 _hv = Vector2.zero;
	
		private Vector2 _rawJHV = Vector2.zero;
		private Vector2 _jHV = Vector2.zero;


		//
		// System methods
		//

		public void Start ()
		{
				minimumDistance = 0.48f;
				shadowOriginalScale = shadow.localScale.x;

				_animator = GetComponent<Animator> ();
				_followScript = GetComponent<CameraFollow> ();
				_tailScript = GetComponent<TailLogic> ();

				foreach (Transform child in transform) {
						if (child.name == "tail anchor") {
								_tailAnchor = child;
						}
				}

				initComponent ();

				_tailScript.addLink ();
				_tailScript.addLink ();
				//_tailScript.addLink ();
				//_tailScript.addLink ();
		}

		public void Update ()
		{
				//Debug.Log(Input.GetJoystickNames()[0]);

				// Axis input handle
				_rawHV.Set (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
				_hv.Set (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
		
				if (Input.GetJoystickNames ().Length > 0) {
						_rawJHV.Set (Input.GetAxisRaw ("JoystickHorizontal"), Input.GetAxisRaw ("JoystickVertical"));
						_jHV.Set (Input.GetAxis ("JoystickHorizontal"), Input.GetAxis ("JoystickVertical"));
				}

				// Jump/tounge input handle
				if (isGrounded && Input.GetKeyDown ("z")) {
						_animator.SetBool ("grounded", false);
						rigidbody.velocity = new Vector3 (0, jumpImpulse, 0);
				}
		}

		private void FixedUpdate ()
		{
				if (_keyToRotation == null)
						initComponent ();

				checkDeathTriggers ();
				updateShadow ();
				checkGroundTriggers ();

				updateMovement ();
		}


		//
		// Logic
		//
		private void updateMovement ()
		{
				float absSpeed;
				float yRotation;
				float speed;
				// joystick axis input is priority:
				if (_rawJHV.magnitude != 0) {
						absSpeed = Mathf.Max (Mathf.Abs (_jHV.x), Mathf.Abs (_jHV.y));
						_animator.SetFloat ("speed", absSpeed);
			
						yRotation = (-1 * Mathf.Atan2 (_jHV.y, _jHV.x) * Mathf.Rad2Deg) - 45;
						//Debug.Log(_jHV + " " + yRotation);
				} else if (_rawHV.magnitude != 0) {
						absSpeed = Mathf.Max (Mathf.Abs (_hv.x), Mathf.Abs (_hv.y));
						_animator.SetFloat ("speed", absSpeed);
			
						yRotation = _keyToRotation [_rawHV.x] [_rawHV.y];

				} else {
						_animator.SetFloat ("speed", 0);
						//rigidbody.velocity = Vector3.zero;
						_tailScript.updateInfo (!isGrounded);
						return;
				}
		
				// keyboard input only if no joystick axis made:
		
				speed = absSpeed * maxSpeed;
		
				float lerpRange = this.lerpAtMinSpeed - this.lerpAtMaxSpeed;
				float lerp = this.isGrounded ? this.lerpAtMaxSpeed + lerpRange - (speed / maxSpeed) * lerpRange : this.lerpInAir;
				//Debug.Log(speed + " " + (lerpAtMaxSpeed + lerpRange - lerp));
		
				float toRot = Mathf.LerpAngle (transform.rotation.eulerAngles.y, yRotation, lerp);
				transform.rotation = Quaternion.identity;
				transform.rotation = Quaternion.Euler (0, toRot, 0);
		
				rigidbody.velocity = new Vector3 (
			Mathf.Sin (toRot * Mathf.Deg2Rad) * speed, 
			rigidbody.velocity.y,
			Mathf.Cos (toRot * Mathf.Deg2Rad) * speed
				);

				_tailScript.updateInfo (true);
		}

		private void checkDeathTriggers ()
		{
				// determine if touching death trigger:
				Collider[] overlapDeath = Physics.OverlapSphere (transform.position, 0.5f, deathTriggerLayer.value);
				//for(int i = 0; i < overlapDeath.Length; i++) {
				//    Debug.Log(overlapDeath[i]);
				//}

				// if fell to the death, reset initial values (stub)
				if (overlapDeath.Length > 0) {
						transform.position = new Vector3 (0, 1f, 0);
						transform.rotation = Quaternion.identity;
						rigidbody.velocity = Vector3.zero;
				}
		}
		
		private void updateShadow ()
		{
				RaycastHit hitInfo = new RaycastHit ();
				bool hit = Physics.Raycast (transform.position, Vector3.down, out hitInfo, Mathf.Infinity, shadowReflectionLayer.value);
				if (hit) {
						shadow.gameObject.SetActive (true);
		
						shadow.position = new Vector3 (hitInfo.point.x, hitInfo.point.y + 0.05f, hitInfo.point.z);
		
						float tScale = Mathf.Min (this.minimumDistance / hitInfo.distance * 2.5f, 1);
						//Debug.Log(minimumDistance / hitInfo.distance * 2);
		
						shadow.localScale = new Vector3 (this.shadowOriginalScale * tScale, 1, this.shadowOriginalScale * tScale);
				} else {
						shadow.gameObject.SetActive (false);
				}
		}

		private void checkGroundTriggers ()
		{
				// TODO: this value should be taken from collider size
				Collider[] overlapGround = Physics.OverlapSphere (transform.position, 0.5f, groundLayer.value);
				//for(int i = 0; i < overlapGround.Length; i++) {
				//	Debug.Log(overlapGround[i]);
				//}
				this.isGrounded = overlapGround.Length > 0;
				if (isGrounded) {
						this._followScript.updateGroundPoint (transform.position.y);
				}
		
				this._animator.SetBool ("grounded", isGrounded);
		}


		//
		// Util methods
		//
		private void initComponent ()
		{
				_cameraDiff = this.mainCamera.transform.rotation.eulerAngles.y;
		
				_keyToRotation = new Dictionary<float, Dictionary<float, float>> ();
		
				_keyToRotation.Add (0, new Dictionary<float, float> ());
				_keyToRotation [0].Add (1, 0f + _cameraDiff);
				_keyToRotation [0].Add (-1, 180f + _cameraDiff);
		
				_keyToRotation.Add (1, new Dictionary<float, float> ());
				_keyToRotation [1].Add (1, 45f + _cameraDiff);
				_keyToRotation [1].Add (0, 90f + _cameraDiff);
				_keyToRotation [1].Add (-1, 135f + _cameraDiff);
		
				_keyToRotation.Add (-1, new Dictionary<float, float> ());
				_keyToRotation [-1].Add (1, -45f + _cameraDiff);
				_keyToRotation [-1].Add (0, -90f + _cameraDiff);
				_keyToRotation [-1].Add (-1, -135f + _cameraDiff);

				_tailScript.injectFirstAnchor (this._tailAnchor, this.transform);
		}
}
