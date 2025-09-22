using UnityEngine;

/// <summary>
/// 플레이어의 상호작용 로직을 담당하는 컴포넌트
/// </summary>
public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionLayerMask = -1; // 모든 레이어와 상호작용
    [SerializeField] private KeyCode interactionKey = KeyCode.F; // 상호작용 키를 F로 변경
    
    [Header("Detection Settings")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private bool showInteractionRange = true;
    
    [Header("UI")]
    // UI는 InteractionUI 클래스에서 관리
    
    private IInteractable currentInteractable;
    private bool isInteractionKeyPressed = false;
    
    private void Awake()
    {
        // 상호작용 포인트가 설정되지 않았다면 자신의 Transform 사용
        if (interactionPoint == null)
        {
            interactionPoint = transform;
        }
        
        // UI는 InteractionUI에서 관리하므로 초기화 불필요
    }
    
    private void Update()
    {
        HandleInput();
        DetectInteractable();
        UpdateUI();
    }
    
    /// <summary>
    /// 입력 처리
    /// </summary>
    private void HandleInput()
    {
        // 키를 누르는 순간
        if (Input.GetKeyDown(interactionKey))
        {
            if (currentInteractable != null && currentInteractable.CanInteract())
            {
                currentInteractable.Interact();
            }
        }
        
        // 키를 떼는 순간
        if (Input.GetKeyUp(interactionKey))
        {
            if (currentInteractable != null)
            {
                currentInteractable.StopInteract();
            }
        }
    }
    
    /// <summary>
    /// 상호작용 가능한 오브젝트 감지
    /// </summary>
    private void DetectInteractable()
    {
        // 이전 프레임의 상호작용 대상
        IInteractable previousInteractable = currentInteractable;
        currentInteractable = null;
        
        // 상호작용 범위 내의 모든 콜라이더 검사
        // LayerMask를 사용하도록 수정하여 성능 최적화
        Collider2D[] colliders = Physics2D.OverlapCircleAll( 
            interactionPoint.position, 
            interactionRange, 
            interactionLayerMask
        );
        
        float closestDistance = float.MaxValue;
        IInteractable closestInteractable = null;
        
        foreach (Collider2D col in colliders)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            // CanInteract() 조건을 제거하여, 상호작용 가능 여부와 상관없이 가장 가까운 오브젝트를 찾도록 수정
            if (interactable != null)
            {
                float distance = Vector2.Distance(interactionPoint.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        currentInteractable = closestInteractable;
        
        // 상호작용 대상이 바뀌었다면 로그 출력
        if (currentInteractable != previousInteractable)
        {
            if (currentInteractable != null)
            {
                Debug.Log($"Can interact with: {currentInteractable.GetInteractionText()}");
            }
        }
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        // currentInteractable이 null이 아니기만 하면 UI를 표시하도록 수정
        if (currentInteractable != null)
        {
            // GetInteractionText()가 비어있지 않을 때만 메시지를 표시
            string text = currentInteractable.GetInteractionText();
            InteractionUI.ShowMessage(text);
        }
        else
        {
            InteractionUI.HideMessage();
        }
    }
    
    /// <summary>
    /// 상호작용 범위 설정
    /// </summary>
    public void SetInteractionRange(float newRange)
    {
        interactionRange = newRange;
    }
    
    /// <summary>
    /// 상호작용 키 설정
    /// </summary>
    public void SetInteractionKey(KeyCode newKey)
    {
        interactionKey = newKey;
    }
    
    /// <summary>
    /// 현재 상호작용 가능한 오브젝트 반환
    /// </summary>
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }
    
    /// <summary>
    /// 강제로 상호작용 실행 (외부에서 호출용)
    /// </summary>
    public void ForceInteract()
    {
        if (currentInteractable != null && currentInteractable.CanInteract())
        {
            currentInteractable.Interact();
        }
    }
    
    /// <summary>
    /// 상호작용 범위 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showInteractionRange) return;
        
        Vector3 position = interactionPoint != null ? interactionPoint.position : transform.position;
        
        Gizmos.color = currentInteractable != null ? Color.green : Color.white;
        Gizmos.DrawWireSphere(position, interactionRange);
        
        // 반투명 원 그리기
        Color fillColor = Gizmos.color;
        fillColor.a = 0.1f;
        Gizmos.color = fillColor;
        Gizmos.DrawSphere(position, interactionRange);
    }
}