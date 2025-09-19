using UnityEngine;

/// <summary>
/// 안전지대의 범위를 정의하고 플레이어의 출입을 감지
/// Box Collider 2D를 사용하여 명확한 사각형 범위 설정
/// </summary>
public class SafeZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Vector2 safeZoneSize = new Vector2(10f, 10f); // 사각형 크기
    
    [Header("Visual Settings")]
    [SerializeField] private Color safeZoneColor = Color.green;
    [SerializeField] private bool showGizmo = true;
    
    [Header("Effects")]
    [SerializeField] private GameObject enterEffect;
    [SerializeField] private GameObject exitEffect;
    
    private bool playerInSafeZone = false;
    
    public bool IsActive 
    { 
        get => isActive; 
        set 
        { 
            isActive = value;
            GetComponent<Collider2D>().enabled = value;
        } 
    }
    
    public bool PlayerInSafeZone => playerInSafeZone;
    
    private void Awake()
    {
        // Collider2D 설정 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        
        // BoxCollider2D의 경우 크기 설정
        if (col is BoxCollider2D boxCol)
        {
            boxCol.size = safeZoneSize;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            playerInSafeZone = true;
            
            // 플레이어 상태 업데이트
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.SetSafeZoneStatus(true);
            }
            
            // 이벤트 발생
            GameEvents.EnteredSafeZone();
            
            // 입장 이펙트
            if (enterEffect != null)
            {
                Instantiate(enterEffect, other.transform.position, Quaternion.identity);
            }
            
            Debug.Log($"Player entered safe zone: {gameObject.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 비활성화된 깃발에서는 안전지대 해제 처리를 하지 않음
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            playerInSafeZone = false;
            
            // 플레이어 상태 업데이트
            PlayerStatus playerStatus = other.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.SetSafeZoneStatus(false);
            }
            
            // 이벤트 발생
            GameEvents.ExitedSafeZone();
            
            // 퇴장 이펙트
            if (exitEffect != null)
            {
                Instantiate(exitEffect, other.transform.position, Quaternion.identity);
            }
            
            Debug.Log($"Player exited safe zone: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 안전지대 활성화
    /// </summary>
    public void ActivateSafeZone()
    {
        IsActive = true;
        Debug.Log($"Safe zone activated: {gameObject.name}");
    }
    
    /// <summary>
    /// 안전지대 비활성화
    /// </summary>
    public void DeactivateSafeZone()
    {
        IsActive = false;
        
        // 플레이어가 안에 있었다면 강제로 나간 것으로 처리
        if (playerInSafeZone)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                OnTriggerExit2D(player.GetComponent<Collider2D>());
            }
        }
        
        Debug.Log($"Safe zone deactivated: {gameObject.name}");
    }
    
    /// <summary>
    /// 안전지대 크기 설정
    /// </summary>
    public void SetSize(Vector2 newSize)
    {
        safeZoneSize = newSize;
        
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.size = safeZoneSize;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        
        Gizmos.color = safeZoneColor;
        
        // 사각형 와이어프레임 그리기
        Vector3 center = transform.position;
        Vector3 size = new Vector3(safeZoneSize.x, safeZoneSize.y, 0);
        
        // 와이어프레임 큐브
        Gizmos.DrawWireCube(center, size);
        
        // 활성화 상태에 따라 색상 변경하여 채우기
        Color fillColor = safeZoneColor;
        fillColor.a = isActive ? 0.2f : 0.1f;
        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, size);
    }
}