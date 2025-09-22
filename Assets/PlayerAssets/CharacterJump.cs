using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 캐릭터의 점프 메커니즘을 관리하는 클래스입니다. 가변 점프, 코요테 시간, 점프 버퍼 등 고급 기능을 포함합니다.
/// </summary>
public class CharacterJump : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    private PlayerStatus playerStatus; // 추가
    private CharacterGroundCheck ground;
    public Vector2 velocity;
    private CharacterJuice juice;
    private CharacterMove move;

    [Header("점프 능력치")]
    [Tooltip("점프의 최대 높이입니다.")]
    public float jumpHeight = 7.3f; 
    [Tooltip("두 번째 점프(공중 점프)의 최대 높이입니다.")]
    public float airJumpHeight = 6f;
    [Tooltip("점프 후 최고점에 도달하는 데 걸리는 시간입니다.")]
    public float timeToJumpApex; 
    [Tooltip("점프 상승 시 중력 계수입니다. 값이 낮을수록 더 오래 떠 있습니다.")]
    public float upwardMovementMultiplier = 1f; 
    [Tooltip("점프 하강 시 중력 계수입니다. 값이 높을수록 더 빨리 떨어집니다.")]
    public float downwardMovementMultiplier = 6f; 
    [Tooltip("공중에서 추가로 점프할 수 있는 횟수입니다. (예: 1로 설정 시 2단 점프 가능)")]
    public int maxAirJumps = 0; 

    [Header("점프 옵션")]
    [Tooltip("점프 키를 누르는 시간에 따라 점프 높이를 조절할지 여부입니다.")]
    public bool variablejumpHeight; 
    [Tooltip("가변 점프 시, 점프 키를 떼었을 때 적용될 중력 계수입니다.")]
    public float jumpCutOff; 
    [Tooltip("최대 하강 속도를 제한합니다.")]
    public float speedLimit; 
    [Tooltip("점프 버퍼: 착지 직전에 점프를 미리 입력할 수 있는 시간입니다.")]
    public float jumpBuffer = 0.15f; 

    [Header("계산된 값 (내부용)")]
    public float jumpSpeed; // 점프에 필요한 초기 속도
    private float defaultGravityScale; // 기본 중력 스케일
    public float gravMultiplier; // 현재 적용 중인 중력 배율

    [Header("현재 상태")]
    public bool canJumpAgain = false; // 공중 추가 점프 가능 여부
    private bool desiredJump; // 플레이어가 점프를 원하는지 여부
    private float jumpBufferCounter; // 점프 버퍼 시간 카운터
    private float coyoteTimeCounter = 0; // 코요테 시간 카운터
    private bool pressingJump; // 현재 점프 키를 누르고 있는지 여부
    public bool onGround; // 현재 땅에 닿아있는지 여부
    private bool currentlyJumping; // 현재 점프 동작 중인지 여부

    private void Awake()
    {
        // 필수 컴포넌트들을 가져옵니다.
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGroundCheck>();
        juice = GetComponent<CharacterJuice>();
        playerStatus = GetComponent<PlayerStatus>(); // 추가
        move = GetComponent<CharacterMove>();
        defaultGravityScale = 1f;
    }

    private void OnEnable()
    {
        // InputManager의 점프 이벤트에 구독합니다.
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SpacePressed += OnJump;
            InputManager.Instance.SpaceReleased += OffJump;
        }
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SpacePressed -= OnJump;
            InputManager.Instance.SpaceReleased -= OffJump;
        }
    }

    public void OnJump()
    {
        // 플레이어가 살아있고, 움직일 수 있을 때만 점프 입력을 받습니다.
        if (MovementLimiter.Instance.CharacterCanMove && (playerStatus == null || !playerStatus.IsDead))
        {
            desiredJump = true;
            pressingJump = true;
        }
    }

    public void OffJump()
    {
        // 플레이어가 살아있고, 움직일 수 있을 때만 점프 입력을 받습니다.
        if (MovementLimiter.Instance.CharacterCanMove && (playerStatus == null || !playerStatus.IsDead))
        {
            pressingJump = false;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        setPhysics();
        onGround = ground.GetOnGround();

        if (jumpBuffer > 0)
        {
            if (desiredJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBuffer)
                {
                    desiredJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            coyoteTimeCounter = 0;
        }
    }

    /// <summary>
    /// 점프 높이와 최고점 도달 시간을 바탕으로 중력을 설정합니다.
    /// </summary>
    private void setPhysics()
    {
        Vector2 newGravity = new Vector2(0, (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex));
        rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

    /// <summary>
    /// 고정된 시간 간격으로 호출되며, 물리 관련 로직을 처리합니다.
    /// </summary>
    private void FixedUpdate()
    {
        velocity = rb.linearVelocity;

        if (desiredJump)
        {
            DoAJump();
            rb.linearVelocity = velocity;
            return;
        }

        calculateGravity();
    }

    /// <summary>
    /// 상황에 따라 적절한 중력 값을 계산하고 적용합니다.
    /// </summary>
    private void calculateGravity()
    {
        // 상승 중일 때
        if (rb.linearVelocity.y > 0.01f) 
        {
            if (onGround)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                // 가변 점프 높이 옵션이 켜져 있을 때
                if (variablejumpHeight)
                {
                    // 점프 키를 계속 누르고 있으면 상승 중력을, 떼면 더 강한 중력을 적용해 상승을 멈춥니다.
                    if (pressingJump && currentlyJumping)
                    {
                        gravMultiplier = upwardMovementMultiplier;
                    }
                    else
                    {
                        gravMultiplier = jumpCutOff;
                    }
                }
                else
                {
                    gravMultiplier = upwardMovementMultiplier;
                }
            }
        }
        // 하강 중일 때
        else if (rb.linearVelocity.y < -0.01f) 
        {

            if (onGround)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                // 더 빨리 떨어지도록 하강 중력을 적용합니다.
                gravMultiplier = downwardMovementMultiplier;
            }

        }
        // 거의 정지 상태일 때 (최고점 근처 또는 바닥)
        else 
        {
            if (onGround)
            {
                currentlyJumping = false;
                canJumpAgain = false;
            }

            gravMultiplier = defaultGravityScale;
        }
        // 속도 제한: 최대 하강 속도를 넘어가지 않도록 합니다.
        rb.linearVelocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, 100));
    }

    /// <summary>
    /// 점프 관련 모든 상태 변수를 초기화합니다.
    /// </summary>
    public void ResetJumpState()
    {
        canJumpAgain = false;
        currentlyJumping = false;
        desiredJump = false;
        pressingJump = false;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
    }

    /// <summary>
    /// 실제 점프를 실행하는 로직입니다.
    /// </summary>
    private void DoAJump()
    {
        // 점프 조건: 땅에 있거나, 코요테 시간 중이거나, 추가 점프가 가능할 때
        if (onGround || (coyoteTimeCounter > 0.03f) || canJumpAgain)
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            // 공중 점프 횟수를 관리합니다.
            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);

            // 현재 점프가 지상 점프인지 공중 점프인지에 따라 다른 점프 높이를 사용합니다.
            float currentJumpHeight = onGround || coyoteTimeCounter > 0.03f ? jumpHeight : airJumpHeight;
            if (onGround || coyoteTimeCounter > 0.03f)
            {
                currentJumpHeight = jumpHeight;
            } else {
                currentJumpHeight = airJumpHeight;
            }

            // 설정된 점프 높이에 도달하기 위한 초기 속도를 계산합니다.
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * rb.gravityScale * currentJumpHeight);

            // 이미 상승 중일 때 점프하면, 현재 속도를 고려하여 점프 힘을 조절합니다.
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            // 하강 중일 때 점프하면, 하강 속도를 상쇄하고 추가로 점프 힘을 더합니다.
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.linearVelocity.y);
            }

            // 계산된 점프 속도를 Rigidbody에 적용합니다.
            velocity.y += jumpSpeed;
            currentlyJumping = true;

            if (juice != null)
            {
                juice.jumpEffects();
            }
        }

        // 점프 버퍼가 0이면, 점프 시도 후 바로 desiredJump를 false로 만듭니다.
        if (jumpBuffer == 0)
        {
            desiredJump = false;
        }
    }

    /// <summary>
    /// 외부에서 호출하여 캐릭터를 위로 튕겨 올립니다. (예: 밟았을 때)
    /// </summary>
    /// <param name="bounceAmount">튕겨 오를 힘의 크기</param>
    public void bounceUp(float bounceAmount)
    {
        rb.AddForce(Vector2.up * bounceAmount, ForceMode2D.Impulse);
    }
}
