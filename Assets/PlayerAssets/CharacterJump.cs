using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterJump : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    private CharacterGroundCheck ground;
    public Vector2 velocity;
    private CharacterJuice juice;
    private CharacterMove move;

    [Header("Jumping Stats")]
    public float jumpHeight = 7.3f;
    public float timeToJumpApex;
    public float upwardMovementMultiplier = 1f;
    public float downwardMovementMultiplier = 6f;
    public int maxAirJumps = 0;

    [Header("Options")]
    public bool variablejumpHeight;
    public float jumpCutOff;
    public float speedLimit;
    public float coyoteTime = 0.15f;
    public float jumpBuffer = 0.15f;

    [Header("Calculations")]
    public float jumpSpeed;
    private float defaultGravityScale;
    public float gravMultiplier;

    [Header("Current State")]
    public bool canJumpAgain = false;
    private bool desiredJump;
    private float jumpBufferCounter;
    private float coyoteTimeCounter = 0;
    private bool pressingJump;
    public bool onGround;
    private bool currentlyJumping;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<CharacterGroundCheck>();
        juice = GetComponent<CharacterJuice>();
        move = GetComponent<CharacterMove>();
        defaultGravityScale = 1f;
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SpacePressed += OnJump;
            InputManager.Instance.SpaceReleased += OffJump;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SpacePressed -= OnJump;
            InputManager.Instance.SpaceReleased -= OffJump;
        }
    }

    public void OnJump()
    {
        Debug.Log("스페이스 누르기 시작");
        if (MovementLimiter.Instance.CharacterCanMove)
        {
            desiredJump = true;
            pressingJump = true;
        }
    }

    public void OffJump()
    {
        Debug.Log("스페이스 누르기 끝");
        if (MovementLimiter.Instance.CharacterCanMove)
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

        // 널널한 점프
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
        // 코요테
        if (!currentlyJumping && !onGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            coyoteTimeCounter = 0;
        }
    }


    private void setPhysics()
    {
        Vector2 newGravity = new Vector2(0, (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex));
        rb.gravityScale = (newGravity.y / Physics2D.gravity.y) * gravMultiplier;
    }

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

    private void calculateGravity()
    {

        if (rb.linearVelocity.y > 0.01f)
        {
            if (onGround)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                if (variablejumpHeight)
                {
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
        else if (rb.linearVelocity.y < -0.01f)
        {

            if (onGround)
            {
                gravMultiplier = defaultGravityScale;
            }
            else
            {
                gravMultiplier = downwardMovementMultiplier;
            }

        }
        else
        {
            if (onGround)
            {
                currentlyJumping = false;
                canJumpAgain = false;
            }

            gravMultiplier = defaultGravityScale;
        }
        rb.linearVelocity = new Vector3(velocity.x, Mathf.Clamp(velocity.y, -speedLimit, 100));
    }

    public void ResetJumpState()
    {
        canJumpAgain = false;
        currentlyJumping = false;
        desiredJump = false;
        pressingJump = false;
        jumpBufferCounter = 0;
        coyoteTimeCounter = 0;
    }

    private void DoAJump()
    {
        if (onGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain)
        {
            desiredJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            canJumpAgain = (maxAirJumps == 1 && canJumpAgain == false);

            //Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * rb.gravityScale * jumpHeight);

            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.linearVelocity.y);
            }

            velocity.y += jumpSpeed;
            currentlyJumping = true;

            if (juice != null)
            {
                juice.jumpEffects();
            }
        }

        if (jumpBuffer == 0)
        {
            desiredJump = false;
        }
    }

    public void bounceUp(float bounceAmount)
    {
        rb.AddForce(Vector2.up * bounceAmount, ForceMode2D.Impulse);
    }
}

