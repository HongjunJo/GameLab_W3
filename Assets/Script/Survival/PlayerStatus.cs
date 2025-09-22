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
    
    private void Awake()
    {
        // Start에서 다시 체크하므로 Awake에서는 경고만 출력
        CheckComponents();
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
        Debug.Log("PlayerStatus: Player death detected");
        
        // DangerGaugeSystem이 있다면 리스폰 처리는 해당 시스템에서 담당
        if (dangerGaugeSystem != null)
        {
            Debug.Log("DangerGaugeSystem will handle respawn");
            return;
        }
    }
    
    /// <summary>
    /// 플레이어가 새로운 리스폰 섹터에 진입했을 때 호출됩니다.
    /// </summary>
    public void EnterRespawnSector(RespawnSector sector)
    {
        // 새로운 섹터에 진입하면 현재 섹터로 설정
        CurrentSector = sector;
        Debug.Log($"플레이어가 '{sector.SectorName}' 섹터에 진입했습니다. 리스폰 포인트: {sector.RespawnPoint.name}");
    }

    /// <summary>
    /// 플레이어가 리스폰 섹터에서 나갔을 때 호출됩니다. (RespawnSector의 OnTriggerExit2D에서 호출)
    /// </summary>
    public void ExitRespawnSector(RespawnSector sector)
    {
        // 현재 플레이어가 속해있다고 기록된 섹터에서 나가는 경우에만 CurrentSector를 null로 설정합니다.
        // 또한, 플레이어가 사망한 상태에서는 리스폰 위치 정보를 유지하기 위해 이 로직을 건너뜁니다.
        if (CurrentSector == sector && !isDead)
        {
            CurrentSector = null;
            Debug.Log($"플레이어가 '{sector.SectorName}' 섹터에서 나갔습니다. 현재 섹터가 없습니다.");
        }
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