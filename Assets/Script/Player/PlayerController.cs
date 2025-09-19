using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 컨트롤러 - Input System을 사용한 2D 횡스크롤 이동
/// 카메라 시스템과 연동을 위한 이동 방향 정보 제공
/// </summary>
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
    
    [Header("Visual Settings")]
    [SerializeField] private float playerZPosition = -1f; // 플레이어가 항상 앞에 보이도록
    
    [Header("Debug Info")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private float currentMoveDirection; // 카메라용

    // Input Actions
    private PlayerControls controls;
    
    // 내부적으로 사용할 변수들
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    
    // 카메라 연동을 위한 공개 프로퍼티
    public float MoveDirection => moveDirection.x;
    public bool IsMoving => Mathf.Abs(moveDirection.x) > 0.1f;
    public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();
        
        // Ground Check Point가 없으면 자동 생성
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
            Debug.Log("PlayerController: GroundCheck automatically created");
        }
    }

    private void Start()
    {
        // 플레이어 Z축 위치 고정 (항상 앞에 보이도록)
        Vector3 pos = transform.position;
        pos.z = playerZPosition;
        transform.position = pos;
        
        // Rigidbody2D Z축 제약 설정 (2D 게임에서 안전)
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // SpriteRenderer 정렬 순서 설정 (대안적 방법)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10; // 다른 오브젝트보다 앞에 표시
        }
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
    
    private void Update()
    {
        // 디버그 정보 업데이트
        currentMoveDirection = moveDirection.x;
        if (rb != null)
        {
            currentVelocity = rb.linearVelocity;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            // 이동 처리
            rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
            
            // 땅 체크
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            
            // 플레이어가 너무 아래로 떨어지면 경고
            if (transform.position.y < -50f)
            {
                Debug.LogWarning($"Player falling! Position: {transform.position}, Velocity: {rb.linearVelocity}");
                Debug.LogWarning($"Ground Check: {isGrounded}, Ground Layer: {groundLayer.value}");
            }
        }
    }

    // Input Event Handlers
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
        Debug.Log($"PlayerController: Move input - {moveDirection.x}");
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveDirection = Vector2.zero;
        Debug.Log("PlayerController: Move input canceled");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log("PlayerController: Jump!");
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        CheckForInteraction();
    }

    /// <summary>
    /// 상호작용 체크
    /// </summary>
    private void CheckForInteraction()
    {
        // 플레이어 앞쪽으로 레이캐스트
        Vector2 rayDirection = moveDirection.x >= 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, interactionDistance, interactionLayer);
        
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact();
                Debug.Log($"PlayerController: Interacted with {hit.collider.name}");
            }
        }
        else
        {
            Debug.Log("PlayerController: No interactable object found");
        }
    }
    
    /// <summary>
    /// 플레이어를 특정 위치로 순간이동 (깃발 시스템용)
    /// </summary>
    public void TeleportTo(Vector3 position)
    {
        transform.position = position;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 속도 리셋
        }
        Debug.Log($"PlayerController: Teleported to {position}");
    }
    
    /// <summary>
    /// 이동 속도 설정 (런타임 조정용)
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        Debug.Log($"PlayerController: Move speed set to {newSpeed}");
    }
    
    /// <summary>
    /// 점프 힘 설정 (런타임 조정용)
    /// </summary>
    public void SetJumpForce(float newJumpForce)
    {
        jumpForce = newJumpForce;
        Debug.Log($"PlayerController: Jump force set to {newJumpForce}");
    }

    private void OnDrawGizmos()
    {
        // Ground Check 범위 표시
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // 상호작용 범위 표시
        Gizmos.color = Color.yellow;
        Vector2 rayDirection = moveDirection.x >= 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rayDirection * interactionDistance);
        
        // 현재 이동 방향 표시
        if (IsMoving)
        {
            Gizmos.color = Color.blue;
            Vector2 moveDir = new Vector2(moveDirection.x, 0).normalized;
            Gizmos.DrawRay(transform.position, moveDir * 2f);
        }
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