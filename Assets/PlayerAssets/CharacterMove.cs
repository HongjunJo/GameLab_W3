using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SocialPlatforms;

public enum PlayerState
{
    Normal, // 일반 이동 및 점프 상태
    Bouncing // 바운스로 튕겨나가는 상태
}

public class CharacterMove : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    CharacterGroundCheck ground;
    private CharacterJump jump; // 추가
    public ParticleSystem deadEffect;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] public float maxAcceleration = 50f;
    [SerializeField, Range(0f, 100f)] public float maxDeceleration = 50f;
    [SerializeField, Range(0f, 100f)] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)] public float maxAirTurnSpeed = 80f;

    [Header("Options")]
    public bool useAcceleration;

    [Header("Calculations")]
    public float directionX;
    private Vector2 desiredVelocity;
    public Vector2 velocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;

    [Header("Current State")]
    public bool onGround;
    public bool pressingKey;
    public float playerSize;

    [Header("Bounce")]
    public float bounceForce = 15f;
    public float bounceDuration = 0.3f;
    public float bounceX = -1f;
    public float bounceY = 1f;
    private bool isBouncing = false;
    private bool isDinoAttacked = false;
    private bool isCleared = false;
    private Coroutine bounceCoroutine;

    public PlayerState currentState = PlayerState.Normal; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGroundCheck>();
        jump = GetComponent<CharacterJump>(); // 추가
    }
    void Start()
    {
        // InputManager가 준비되었을 때만 이벤트 등록
        if (InputManager.Instance != null)
        {
            InputManager.Instance.MovementAD += OnMovement;
        }
    }
    private void OnEnable()
    {
        // 컴포넌트가 활성화될 때 이벤트 다시 등록
        if (InputManager.Instance != null)
        {
            InputManager.Instance.MovementAD += OnMovement;
        }
    }

    private void OnDisable()
    {
        // 컴포넌트가 비활성화될 때 이벤트 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.MovementAD -= OnMovement;
        }
    }

    public void OnMovement(float movement)
    {
        if (MovementLimiter.Instance.CharacterCanMove&&!isCleared&&!isDinoAttacked)
        {
            directionX = movement;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!MovementLimiter.Instance.CharacterCanMove)
        {
            directionX = 0;
        }

        if (directionX != 0)
        {
            transform.localScale = new Vector3(directionX > 0 ? playerSize : -playerSize, playerSize, 1);
            pressingKey = true;
        }
        else
        {
            pressingKey = false;
        }

        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed, 0f);
    }

    private void FixedUpdate()
    {
        if (currentState == PlayerState.Normal)
        {
            onGround = ground.GetOnGround();
            velocity = rb.linearVelocity;
            if (useAcceleration)
            {
                runWithAcceleration();
            }
            else
            {
                if (onGround) { runWithoutAcceleration(); }
                else { runWithAcceleration(); }
            }
        }
    }

    private void runWithAcceleration()
    {
        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDeceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (pressingKey) { 
            if(Mathf.Sign(directionX) != Mathf.Sign(velocity.x)) {
                maxSpeedChange = turnSpeed * Time.deltaTime;
            }
            else { 
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        }
        else
        {
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

        rb.linearVelocity = velocity;
    }

    private void runWithoutAcceleration()
    {
        velocity.x = desiredVelocity.x;

        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void ApplyBounce(GameObject gameObject, Collision2D collision)
    {
        currentState = PlayerState.Bouncing;

        if (jump != null)
        {
            jump.ResetJumpState();
        }

        Vector2 pushDirection = collision.contacts[0].normal;
        transform.position += (Vector3)pushDirection * 0.05f; 

        rb.linearVelocity = Vector2.zero; 
        Vector2 bounceDirection = new Vector2(bounceX, bounceY).normalized; 
        rb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
        StartCoroutine(BounceRoutine(gameObject));
    }


    // 일정 시간 동안 조작을 막는 코루틴
    private IEnumerator BounceRoutine(GameObject gameObject)
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<PolygonCollider2D>().enabled = true;
        yield return new WaitUntil(() => ground.GetOnGround());

        yield return new WaitForSeconds(0.1f);

        currentState = PlayerState.Normal;


        if (ground.GetOnGround() && jump != null)
        {
            jump.canJumpAgain = false;
        }
    }
}
