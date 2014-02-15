using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour {

    private static string SIN_KEY = "sin";
    private static string COS_KEY = "cos";

    public float maxSpeed = 0f;
    public float jumpImpulse = 12f;
    public Camera mainCamera;
    public LayerMask whatIsGround;

    private bool isGrounded = false;

    private Animator _animator;
    private Transform _container;

    private Dictionary<float, Dictionary<float, float>> _keyToRotation;
    private Dictionary<float, Dictionary<string, float>> _rotationToSinCos;

	// Use this for initialization
	void Start () {

        fillMoveValues();

        _animator = GetComponent<Animator>();
        _container = GetComponent<Transform>();
	}

    private void fillMoveValues() {
        float cameraDiff = this.mainCamera.transform.rotation.eulerAngles.y;

        _keyToRotation = new Dictionary<float, Dictionary<float, float>>();

        _keyToRotation.Add(0, new Dictionary<float, float>());
        _keyToRotation[0].Add(1, 0f + cameraDiff);
        _keyToRotation[0].Add(-1, 180f + cameraDiff);

        _keyToRotation.Add(1, new Dictionary<float, float>());
        _keyToRotation[1].Add(1, 45f + cameraDiff);
        _keyToRotation[1].Add(0, 90f + cameraDiff);
        _keyToRotation[1].Add(-1, 135f + cameraDiff);

        _keyToRotation.Add(-1, new Dictionary<float, float>());
        _keyToRotation[-1].Add(1, -45f + cameraDiff);
        _keyToRotation[-1].Add(0, -90f + cameraDiff);
        _keyToRotation[-1].Add(-1, -135f + cameraDiff);

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

    private Vector2 lastSpeed = Vector2.zero;
    private Vector2 lastRawSpeed = Vector2.zero;

    private float escapeSeqTimer = 0;
    private float directionChange = 0;

    private void FixedUpdate(){
        if (_keyToRotation == null) fillMoveValues();

        //определяем, на земле ли персонаж

        isGrounded = Physics.OverlapSphere(transform.position, 0.5f, whatIsGround.value).Length > 0; 
        //устанавливаем соответствующую переменную в аниматоре
        _animator.SetBool ("grounded", isGrounded);
        //устанавливаем в аниматоре значение скорости взлета/падения
        //anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
        //если персонаж в прыжке - выход из метода, чтобы не выполнялись действия, связанные с бегом
        //if (!isGrounded)

        float rawV = Input.GetAxisRaw("Vertical");
        float rawH = Input.GetAxisRaw("Horizontal");

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        float absSpeed = Mathf.Max(Mathf.Abs(h), Mathf.Abs(v));
        _animator.SetFloat("speed", absSpeed);

        if (rawH == 0 && rawV == 0) {
            if (escapeSeqTimer > 0) {
                //Debug.Log("ESCAPE SEQ TIME: " + (Time.realtimeSinceStartup - escapeSeqTimer));
            }
            escapeSeqTimer = 0;
            lastRawSpeed.Set(rawH, rawV);
            lastSpeed.Set(h, v);
            return;
        }

        if (lastRawSpeed.x != 0 && rawH == 0 && rawV != 0) {
            //Debug.Log("escape sequence in motion! from H");
            escapeSeqTimer = Time.realtimeSinceStartup;
        }
        if (lastRawSpeed.y != 0 && rawV == 0 && rawH != 0) {
            //Debug.Log("escape sequence in motion! from V");
            escapeSeqTimer = Time.realtimeSinceStartup;
        }

        float yRotation = _keyToRotation[rawH][rawV];
        transform.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
        

        float speed = absSpeed * maxSpeed;
        rigidbody.velocity = new Vector3(
            _rotationToSinCos[yRotation][SIN_KEY] * speed, 
            rigidbody.velocity.y,
            _rotationToSinCos[yRotation][COS_KEY] * speed
        );

        lastSpeed.Set(h, v);
        lastRawSpeed.Set(rawH, rawV);
    }
	
	// Update is called once per frame
    void Update() {
        //если персонаж на земле и нажат пробел...
        if (this.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
            //Debug.Log("OKOKO");
            //устанавливаем в аниматоре переменную в false
            _animator.SetBool("grounded", false);
            //прикладываем силу вверх, чтобы персонаж подпрыгнул
            //rigidbody.AddForce(new Vector3(0, 600f, 0));
            rigidbody.velocity = new Vector3(0, jumpImpulse, 0);
        }
    }
}
