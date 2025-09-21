using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태를 관리하는 컴포넌트
/// </summary>
public class PlayerStatus : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private bool isDead = false;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform lastSafeZonePosition;
    [SerializeField] private SafeZone lastSafeZone;
    public bool IsDead => isDead;
    public Transform LastSafeZonePosition => lastSafeZonePosition;
    public SafeZone LastSafeZone => lastSafeZone;
    
    private Health healthComponent;
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
        healthComponent = GetComponent<Health>();
        dangerGaugeSystem = GetComponent<DangerGaugeSystem>();
        
        if (healthComponent == null && dangerGaugeSystem == null)
        {
            Debug.LogWarning("PlayerStatus: No Health or DangerGaugeSystem found. Waiting for SystemTransitionManager...");
        }
        else if (dangerGaugeSystem != null)
        {
            Debug.Log("PlayerStatus using DangerGaugeSystem");
        }
        else if (healthComponent != null)
        {
            Debug.Log("PlayerStatus using Health component");
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
    /// 현재 안전지대 위치 기록
    /// </summary>
    private void RecordCurrentSafeZone()
    {
        lastSafeZonePosition = transform;
        
        // 현재 있는 SafeZone 찾기
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (var col in colliders)
        {
            SafeZone safeZone = col.GetComponent<SafeZone>();
            if (safeZone != null && safeZone.IsActive)
            {
                lastSafeZone = safeZone;
                lastSafeZonePosition = safeZone.transform;
                break;
            }
        }
        
        Debug.Log($"Recorded safe zone position: {lastSafeZonePosition.position}");
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
        
        // Health 시스템만 있는 경우에만 기존 리스폰 로직 사용
        if (healthComponent != null)
        {
            Invoke(nameof(RespawnPlayer), 1f);
        }
    }
    
    /// <summary>
    /// 플레이어 리스폰 (Health 시스템용)
    /// </summary>
    private void RespawnPlayer()
    {
        if (lastSafeZonePosition != null)
        {
            // 마지막 안전지대로 이동
            transform.position = lastSafeZonePosition.position;
            
            // 체력 회복
            if (healthComponent != null)
            {
                healthComponent.Revive();
            }
            
            // 상태 초기화
            isDead = false;
            
            Debug.Log($"Player respawned at: {transform.position}");
        }
        else
        {
            Debug.LogError("No safe zone recorded for respawn!");
        }
    }
    
    /// <summary>
    /// DangerGaugeSystem에서 리스폰 완료 시 호출
    /// </summary>
    public void OnRespawnCompleted()
    {
        isDead = false;
        // 리스폰 시 새로운 안전지대 위치 기록
        // DangerGaugeSystem이 리스폰 위치를 결정하므로, 그 후에 호출됨
        if (dangerGaugeSystem != null && dangerGaugeSystem.IsAlive)
        {
            RecordCurrentSafeZone();
        }
        Debug.Log("PlayerStatus: Respawn completed");
    }
    
    /// <summary>
    /// 수동으로 리스폰 포인트 설정
    /// </summary>
    public void SetRespawnPoint(Transform newRespawnPoint, SafeZone safeZone = null)
    {
        lastSafeZonePosition = newRespawnPoint;
        lastSafeZone = safeZone;
        Debug.Log($"Respawn point set to: {newRespawnPoint.position}");
    }
    
    /// <summary>
    /// 현재 상태 정보 반환 (디버그용)
    /// </summary>
    public string GetStatusInfo()
    {
        bool currentSafeZoneStatus = false;
        if (dangerGaugeSystem != null)
        {
            // 실시간 상태는 DangerGaugeSystem에서 가져옴
            // dangerGaugeSystem.isInSafeZone은 private이므로 직접 접근 불가.
            // 필요하다면 public 프로퍼티로 노출해야 함. 여기서는 디버그 정보이므로 일단 false로 둠.
        }
        return $"IsDead: {isDead}, LastSafeZone: {(lastSafeZone != null ? lastSafeZone.name : "None")}";
    }
}