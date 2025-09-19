using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer = 1;

    // Public Properties for Camera System (기존 PlayerController와 동일한 인터페이스)
    public float MoveDirection { get; private set; }
    public bool IsMoving { get; private set; }
    public Vector2 Velocity => rb.linearVelocity;

    // Components
    private Rigidbody2D rb;
    private Collider2D col;

    // Input
    private Vector2 moveInput;
    private bool jumpInput;

    // Ground Check
    private bool isGrounded;

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
        }
        
        if (col == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    private void Update()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
    }

    #endregion

    #region Input System Events

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        MoveDirection = moveInput.x;
        IsMoving = Mathf.Abs(MoveDirection) > 0.1f;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpInput = true;
        }
    }

    #endregion

    #region Movement & Jump

    private void HandleMovement()
    {
        float horizontalMove = MoveDirection * moveSpeed;
        rb.linearVelocity = new Vector2(horizontalMove, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        jumpInput = false;
    }

    private void CheckGrounded()
    {
        Vector2 position = transform.position;
        Vector2 direction = Vector2.down;
        float distance = 1.1f;

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer);
        isGrounded = hit.collider != null;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        // Draw ground check ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * 1.1f;
        Gizmos.DrawLine(start, end);
    }

    #endregion
}