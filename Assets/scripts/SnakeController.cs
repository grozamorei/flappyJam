using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CameraFollow))]
public class SnakeController : MonoBehaviour {

    private static string SIN_KEY = "sin";
    private static string COS_KEY = "cos";

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

    private bool isGrounded = false;
    private CameraFollow _followScript;

    private Animator _animator;

    private float minimumDistance;
    private float shadowOriginalScale;

    private float _cameraDiff;
    private Dictionary<float, Dictionary<float, float>> _keyToRotation;
    private Dictionary<float, Dictionary<string, float>> _rotationToSinCos;

	// Use this for initialization
	void Start () {
        minimumDistance = 0.48f;
        shadowOriginalScale = shadow.localScale.x;

        fillMoveValues();

        _animator = GetComponent<Animator>();
        _followScript = GetComponent<CameraFollow>();
	}

    private void fillMoveValues() {
        _cameraDiff = this.mainCamera.transform.rotation.eulerAngles.y;

        _keyToRotation = new Dictionary<float, Dictionary<float, float>>();

        _keyToRotation.Add(0, new Dictionary<float, float>());
        _keyToRotation[0].Add(1, 0f + _cameraDiff);
        _keyToRotation[0].Add(-1, 180f + _cameraDiff);

        _keyToRotation.Add(1, new Dictionary<float, float>());
        _keyToRotation[1].Add(1, 45f + _cameraDiff);
        _keyToRotation[1].Add(0, 90f + _cameraDiff);
        _keyToRotation[1].Add(-1, 135f + _cameraDiff);

        _keyToRotation.Add(-1, new Dictionary<float, float>());
        _keyToRotation[-1].Add(1, -45f + _cameraDiff);
        _keyToRotation[-1].Add(0, -90f + _cameraDiff);
        _keyToRotation[-1].Add(-1, -135f + _cameraDiff);

        _rotationToSinCos = new Dictionary<float, Dictionary<string, float>>();
        for (int i = -1; i <= 1; i++ ) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue;

                float value = _keyToRotation[i][j];
                _rotationToSinCos.Add(value, new Dictionary<string, float>());
                _rotationToSinCos[value].Add(SIN_KEY, Mathf.Sin(Mathf.Deg2Rad * value));
                _rotationToSinCos[value].Add(COS_KEY, Mathf.Cos(Mathf.Deg2Rad * value));
            }
        }
    }

    private void FixedUpdate(){
        if (_keyToRotation == null) fillMoveValues();

        // determine if fall off:
        Collider[] overlapDeath = Physics.OverlapSphere(transform.position, 0.5f, deathTriggerLayer.value);
        //for(int i = 0; i < overlapDeath.Length; i++) {
        //    Debug.Log(overlapDeath[i]);
        //}
        if(overlapDeath.Length > 0) {
            transform.position = new Vector3(0, 1f, 0);
            transform.rotation = Quaternion.identity;
            rigidbody.velocity = Vector3.zero;
            return;
        }

        // updating shadow
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, Mathf.Infinity, shadowReflectionLayer.value);
        if(hit) {
            shadow.gameObject.SetActive(true);

            shadow.position = new Vector3(hitInfo.point.x, hitInfo.point.y + 0.05f, hitInfo.point.z);

            float tScale = Mathf.Min(this.minimumDistance / hitInfo.distance * 2.5f, 1);
            //Debug.Log(minimumDistance / hitInfo.distance * 2);

            shadow.localScale = new Vector3(this.shadowOriginalScale * tScale, 1, this.shadowOriginalScale * tScale);
        }
        else {
            shadow.gameObject.SetActive(false);
        }

        // determine if touching ground:
        Collider[] overlapGround = Physics.OverlapSphere(transform.position, 0.5f, groundLayer.value);
        for(int i = 0; i < overlapGround.Length; i++) {
            //Debug.Log(overlapGround[i]);
        }
        isGrounded = overlapGround.Length > 0;
        if(isGrounded) {
            _followScript.updateGroundPoint(transform.position.y);
        }

        _animator.SetBool ("grounded", isGrounded);

        float absSpeed;
        float yRotation;
        float speed;
        // joystick axis input is priority:
        if(_rawJHV.magnitude != 0) {
            absSpeed = Mathf.Max(Mathf.Abs(_jHV.x), Mathf.Abs(_jHV.y));
            _animator.SetFloat("speed", absSpeed);

            yRotation = (-1 * Mathf.Atan2(_jHV.y, _jHV.x) * Mathf.Rad2Deg) - 45;
            //Debug.Log(_jHV + " " + yRotation);
        } else 
        if (_rawHV.magnitude != 0) {
            absSpeed = Mathf.Max(Mathf.Abs(_hv.x), Mathf.Abs(_hv.y));
            _animator.SetFloat("speed", absSpeed);

            yRotation = _keyToRotation[_rawHV.x][_rawHV.y];

        } else {
            _animator.SetFloat("speed", 0);
            return;
        }
        
        // keyboard input only if no joystick axis made:

        speed = absSpeed * maxSpeed;
        
        float lerpRange = this.lerpAtMinSpeed - this.lerpAtMaxSpeed;
        float lerp = this.isGrounded ? this.lerpAtMaxSpeed + lerpRange - (speed / maxSpeed) * lerpRange : this.lerpInAir;
        //Debug.Log(speed + " " + (lerpAtMaxSpeed + lerpRange - lerp));

        float toRot = Mathf.LerpAngle(transform.rotation.eulerAngles.y, yRotation, lerp);
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0, toRot, 0);

        rigidbody.velocity = new Vector3(
            Mathf.Sin(toRot * Mathf.Deg2Rad) * speed, 
            rigidbody.velocity.y,
            Mathf.Cos(toRot * Mathf.Deg2Rad) * speed
        );
    }

    private Vector2 _rawHV = Vector2.zero;
    private Vector2 _hv = Vector2.zero;

    private Vector2 _rawJHV = Vector2.zero;
    private Vector2 _jHV = Vector2.zero;
	
	// Update is called once per frame
    void Update() {
        //Debug.Log(Input.GetJoystickNames()[0]);

        _rawHV.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _hv.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if(Input.GetJoystickNames().Length > 0) {
            _rawJHV.Set(Input.GetAxisRaw("JoystickHorizontal"), Input.GetAxisRaw("JoystickVertical"));
            _jHV.Set(Input.GetAxis("JoystickHorizontal"), Input.GetAxis("JoystickVertical"));
        }

        //если персонаж на земле и нажат пробел...
        if(this.isGrounded && Input.GetAxis("Jump") > 0) {
            //устанавливаем в аниматоре переменную в false
            _animator.SetBool("grounded", false);
            //прикладываем силу вверх, чтобы персонаж подпрыгнул
            rigidbody.velocity = new Vector3(0, jumpImpulse, 0);
        }
    }
}
