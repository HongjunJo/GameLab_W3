using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 1.5f;
    [SerializeField] private LayerMask interactionLayer;

    // Input Actions
    private PlayerControls controls;
    
    // 내부적으로 사용할 변수들
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
        
        // 이벤트 구독
        controls.Player.Move.performed += OnMovePerformed;
        controls.Player.Move.canceled += OnMoveCanceled;
        controls.Player.Jump.performed += OnJumpPerformed;
        controls.Player.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        controls.Player.Move.performed -= OnMovePerformed;
        controls.Player.Move.canceled -= OnMoveCanceled;
        controls.Player.Jump.performed -= OnJumpPerformed;
        controls.Player.Interact.performed -= OnInteractPerformed;
        
        controls.Disable();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // Input Event Handlers
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveDirection = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        CheckForInteraction();
    }

    private void CheckForInteraction()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, interactionDistance, interactionLayer);
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.right * interactionDistance);
    }
}

public interface IInteractable
{
    /// <summary>
    /// 상호작용이 가능한지 확인
    /// </summary>
    bool CanInteract();
    
    /// <summary>
    /// 상호작용 실행
    /// </summary>
    void Interact();
    
    /// <summary>
    /// 상호작용 UI에 표시될 텍스트
    /// </summary>
    string GetInteractionText();
}