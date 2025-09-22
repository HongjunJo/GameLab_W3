using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태를 관리하는 컴포넌트
/// </summary>
public class PlayerStatus : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private bool isDead = false;
    public bool IsDead => isDead;
    public RespawnSector CurrentSector { get; private set; }
    
    private DangerGaugeSystem dangerGaugeSystem;
    private TemporaryInventory tempInventory;
    
    private void Awake()
    {
        // Start에서 다시 체크하므로 Awake에서는 경고만 출력
        CheckComponents();
        tempInventory = GetComponent<TemporaryInventory>();
    }
    
    private void Start()
    {
        // SystemTransitionManager가 컴포넌트를 추가한 후에 다시 체크
        CheckComponents();
    }
    
    private void CheckComponents()
    {
        // Health 또는 DangerGaugeSystem 중 하나라도 있으면 OK
        dangerGaugeSystem = GetComponent<DangerGaugeSystem>();
        
        if (dangerGaugeSystem == null)
        {
            Debug.LogWarning("PlayerStatus: DangerGaugeSystem not found. Waiting for SystemTransitionManager...");
        }
        else
        {
            Debug.Log("PlayerStatus using DangerGaugeSystem");
        }
    }
    
    /// <summary>
    /// 외부에서 컴포넌트 체크를 강제할 때 사용
    /// </summary>
    public void RefreshComponents()
    {
        CheckComponents();
    }
    
    private void OnEnable()
    {
        // 플레이어 사망 이벤트 구독
        GameEvents.OnPlayerDied += HandlePlayerDeath;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= HandlePlayerDeath;
    }
    
    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void HandlePlayerDeath()
    {
        isDead = true;

        // 임시 인벤토리가 있다면, 내용물을 모두 잃고 리스폰 처리
        if (tempInventory != null)
        {
            tempInventory.ClearAndRespawnAll();
        }
        Debug.Log("PlayerStatus: Player death detected");
        
        // DangerGaugeSystem이 있다면 리스폰 처리는 해당 시스템에서 담당
        if (dangerGaugeSystem != null)
        {
            Debug.Log("DangerGaugeSystem will handle respawn");
            return;
        }
    }
    
    /// <summary>
    /// 플레이어가 리스폰 섹터 안에 있을 때 매 프레임 호출됩니다.
    /// </summary>
    public void UpdateCurrentSector(RespawnSector sector)
    {
        // 현재 섹터가 변경될 때만 로그를 출력하여 중복을 방지합니다.
        if (CurrentSector != sector)
        {
            CurrentSector = sector;

            // 메인 섹터에 진입하면 임시 인벤토리의 자원을 모두 저장합니다.
            if (sector.isMainSector && tempInventory != null)
            {
                Debug.Log($"메인 섹터 '{sector.SectorName}' 진입. 임시 자원을 저장합니다.");
                tempInventory.DepositAllToMainInventory();
            }
            Debug.Log($"플레이어가 '{sector.SectorName}' 섹터에 진입했습니다. 리스폰 포인트: {sector.RespawnPoint.name}");
        }
        
    }

    /// <summary>
    /// 플레이어가 리스폰 섹터에서 벗어났을 때 호출됩니다.
    /// </summary>
    public void ClearCurrentSector()
    {
        if (CurrentSector == null) return;
        Debug.Log($"플레이어가 '{CurrentSector.SectorName}' 섹터에서 벗어났습니다.");
        CurrentSector = null;
    }
    
    /// <summary>
    /// DangerGaugeSystem에서 리스폰 완료 시 호출
    /// </summary>
    public void OnRespawnCompleted()
    {
        isDead = false;
        Debug.Log("PlayerStatus: Respawn completed");
    }
    
    /// <summary>
    /// 현재 상태 정보 반환 (디버그용)
    /// </summary>
    public string GetStatusInfo()
    {
        return $"IsDead: {isDead}, CurrentSector: {(CurrentSector != null ? CurrentSector.SectorName : "None")}";
    }
}