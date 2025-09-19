using UnityEngine;

/// <summary>
/// 깃발 - 안전지대를 제공하고 다음 지역으로의 진행을 관리
/// </summary>
public class Flag : MonoBehaviour, IInteractable
{
    [Header("Flag Settings")]
    [SerializeField] private BuildingRecipe activationRecipe;
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isMainFlag = false; // 시작 깃발인지
    
    [Header("Safe Zone")]
    [SerializeField] private SafeZone safeZone;
    [SerializeField] private Vector2 safeZoneSize = new Vector2(10f, 10f); // Box Collider용 크기
    
    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private GameObject flagLight; // Light GameObject for visual effect
    
    [Header("Progression")]
    [SerializeField] private Flag nextFlag;
    [SerializeField] private Transform playerSpawnPoint;
    
    public bool IsActive => isActive;
    public SafeZone SafeZone => safeZone;
    public Flag NextFlag => nextFlag;
    
    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        // SafeZone 컴포넌트 자동 설정
        if (safeZone == null)
        {
            safeZone = GetComponent<SafeZone>();
            if (safeZone == null)
            {
                safeZone = gameObject.AddComponent<SafeZone>();
            }
        }
        
        // 메인 깃발은 처음부터 활성화
        if (isMainFlag)
        {
            ActivateFlag();
        }
        else
        {
            UpdateVisual();
        }
    }
    
    private void Start()
    {
        // 콜라이더 설정
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
        
        // SafeZone 크기 설정
        if (safeZone != null)
        {
            safeZone.SetSize(safeZoneSize);
        }
    }
    
    public bool CanInteract()
    {
        if (isActive) return false; // 이미 활성화된 깃발은 상호작용 불가
        if (isMainFlag) return false; // 메인 깃발은 상호작용 불가
        
        // 활성화 레시피가 있고 비용을 지불할 수 있는지 확인
        return activationRecipe != null && activationRecipe.CanAfford();
    }
    
    public void Interact()
    {
        if (CanInteract())
        {
            ActivateFlag();
        }
    }
    
    public string GetInteractionText()
    {
        if (isActive)
        {
            return "Active Flag";
        }
        
        if (isMainFlag)
        {
            return "Main Base";
        }
        
        if (activationRecipe != null)
        {
            if (activationRecipe.CanAfford())
            {
                return "Activate Flag";
            }
            else
            {
                return "Need more resources to activate";
            }
        }
        
        return "Inactive Flag";
    }
    
    /// <summary>
    /// 깃발 활성화
    /// </summary>
    public void ActivateFlag()
    {
        // 메인 깃발이 아닌 경우에만 비용 소모
        if (!isMainFlag && activationRecipe != null)
        {
            if (!activationRecipe.ConsumeCost()) return;
        }
        
        isActive = true;
        
        // SafeZone 활성화
        if (safeZone != null)
        {
            safeZone.ActivateSafeZone();
        }
        
        // 이펙트는 사용하지 않음 (Material 기반 시각화만 사용)
        
        // 비주얼 업데이트
        UpdateVisual();
        
        // 이벤트 발생
        GameEvents.BuildingActivated($"Flag_{gameObject.name}");
        
        Debug.Log($"Flag activated: {gameObject.name}");
    }
    
    /// <summary>
    /// 깃발 비활성화
    /// </summary>
    public void DeactivateFlag()
    {
        if (isMainFlag) return; // 메인 깃발은 비활성화 불가
        
        isActive = false;
        
        // SafeZone 비활성화
        if (safeZone != null)
        {
            safeZone.DeactivateSafeZone();
        }
        
        // 비주얼 업데이트
        UpdateVisual();
        
        Debug.Log($"Flag deactivated: {gameObject.name}");
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (objectRenderer != null)
        {
            if (isActive && activeMaterial != null)
            {
                objectRenderer.material = activeMaterial;
            }
            else if (inactiveMaterial != null)
            {
                objectRenderer.material = inactiveMaterial;
            }
        }
        
        // 라이트 설정
        if (flagLight != null)
        {
            flagLight.SetActive(isActive);
            // 라이트의 색상을 변경하려면 Light 컴포넌트나 SpriteRenderer를 사용
            SpriteRenderer lightRenderer = flagLight.GetComponent<SpriteRenderer>();
            if (lightRenderer != null)
            {
                lightRenderer.color = isActive ? Color.green : Color.red;
            }
        }
    }
    
    /// <summary>
    /// 다음 깃발 설정
    /// </summary>
    public void SetNextFlag(Flag flag)
    {
        nextFlag = flag;
    }
    
    /// <summary>
    /// 플레이어 스폰 포인트 가져오기
    /// </summary>
    public Vector3 GetSpawnPosition()
    {
        if (playerSpawnPoint != null)
        {
            return playerSpawnPoint.position;
        }
        return transform.position;
    }
    
    /// <summary>
    /// 플레이어를 이 깃발로 이동
    /// </summary>
    public void TeleportPlayerHere()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = GetSpawnPosition();
            
            // PlayerStatus에 리스폰 포인트 설정
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.SetRespawnPoint(transform, safeZone);
            }
            
            Debug.Log($"Player teleported to {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 안전지대 크기 설정
    /// </summary>
    public void SetSafeZoneSize(Vector2 size)
    {
        safeZoneSize = size;
        if (safeZone != null)
        {
            safeZone.SetSize(size);
        }
    }
    
    /// <summary>
    /// 테스트용 활성화
    /// </summary>
    [ContextMenu("Activate Flag (Test)")]
    public void ActivateFlagTest()
    {
        ActivateFlag();
    }
    
    private void OnDrawGizmosSelected()
    {
        // 안전지대 범위 표시 (Box 형태)
        Gizmos.color = isActive ? Color.green : Color.red;
        
        Vector3 center = transform.position;
        Vector3 size = new Vector3(safeZoneSize.x, safeZoneSize.y, 0);
        
        // 와이어프레임 박스
        Gizmos.DrawWireCube(center, size);
        
        // 다음 깃발로의 연결선 표시
        if (nextFlag != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, nextFlag.transform.position);
        }
    }
}