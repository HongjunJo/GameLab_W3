using UnityEngine;

/// <summary>
/// 안전지대 밖에서 지속적인 위험도 증가를 처리하는 시스템
/// </summary>
public class HPDrainSystem : MonoBehaviour
{
    [Header("Target Player")]
    [SerializeField] private GameObject playerObject; // Player 오브젝트 참조
    
    [Header("Drain Settings")]
    [SerializeField] private float drainAmount = 5f;
    [SerializeField] private float drainInterval = 1f;
    [SerializeField] private bool enableDrain = true;
    
    [Header("Grace Period")]
    [SerializeField] private float gracePeriod = 3f; // 안전지대를 벗어난 후 데미지를 받기 시작하는 시간
    
    [Header("Debug Info")]
    [SerializeField] private float timeSinceLeftSafeZone = 0f;
    [SerializeField] private bool isDraining = false;
    
    private PlayerStatus playerStatus;
    private DangerGaugeSystem dangerGaugeSystem; // DangerGaugeSystem 참조 추가
    private float drainTimer = 0f;
    
    private void Awake()
    {
        // Player 오브젝트가 설정되지 않았다면 태그로 찾기
        if (playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (playerObject == null)
        {
            Debug.LogError("HPDrainSystem: Player object not found! Please assign Player Object in inspector or ensure Player has 'Player' tag.");
            return;
        }
        
        playerStatus = playerObject.GetComponent<PlayerStatus>();
        dangerGaugeSystem = playerObject.GetComponent<DangerGaugeSystem>(); // 컴포넌트 가져오기
        
        if (dangerGaugeSystem == null)
        {
            Debug.LogWarning("HPDrainSystem: DangerGaugeSystem not found on Player. This system will be disabled by SystemTransitionManager if not used.");
        }

        // DangerGaugeSystem이 활성화된 경우, 이 시스템은 비활성화되어야 함
        if (dangerGaugeSystem != null && dangerGaugeSystem.enabled)
        {
            this.enabled = false;
        }
    }
    
    private void OnEnable()
    {
        GameEvents.OnEnteredSafeZone += StopDrain;
        GameEvents.OnExitedSafeZone += StartDrainCountdown;
    }
    
    private void OnDisable()
    {
        GameEvents.OnEnteredSafeZone -= StopDrain;
        GameEvents.OnExitedSafeZone -= StartDrainCountdown;
    }
    
    private void Update()
    {
        // DangerGaugeSystem이 활성화된 경우 이 시스템은 작동하지 않음
        // SystemTransitionManager에 의해 제어되므로, 여기서는 기본적인 활성화 여부와 상태만 체크합니다.
        if (!enableDrain || playerStatus == null || dangerGaugeSystem == null) return;
        
        // 플레이어가 죽었거나, 안전지대에 있다면 위험도 증가 로직을 실행하지 않습니다.
        if (playerStatus.IsDead || dangerGaugeSystem.IsInSafeZone) return;
        
        // 안전지대를 벗어난 시간 계산
        timeSinceLeftSafeZone += Time.deltaTime;
        
        // 유예 시간이 지났다면 드레인 시작
        if (timeSinceLeftSafeZone >= gracePeriod)
        {
            isDraining = true;
            drainTimer += Time.deltaTime;
            
            if (drainTimer >= drainInterval)
            {
                IncreasePlayerDanger();
                drainTimer = 0f;
            }
        }
        else
        {
            isDraining = false;
        }
    }
    
    /// <summary>
    /// 플레이어의 위험도를 증가시킴
    /// </summary>
    private void IncreasePlayerDanger()
    {
        if (dangerGaugeSystem != null && dangerGaugeSystem.IsAlive)
        {
            dangerGaugeSystem.IncreaseDanger(drainAmount);
            Debug.Log($"Danger increased! +{drainAmount} (Time outside: {timeSinceLeftSafeZone:F1}s)");
        }
    }
    
    /// <summary>
    /// 드레인 중단 (안전지대 진입시)
    /// </summary>
    private void StopDrain()
    {
        timeSinceLeftSafeZone = 0f;
        drainTimer = 0f;
        isDraining = false;
        Debug.Log("Danger increase stopped - entered safe zone");
    }
    
    /// <summary>
    /// 드레인 카운트다운 시작 (안전지대 탈출시)
    /// </summary>
    private void StartDrainCountdown()
    {
        timeSinceLeftSafeZone = 0f;
        drainTimer = 0f;
        isDraining = false;
        Debug.Log($"Left safe zone - Danger increase will start in {gracePeriod} seconds");
    }
    
    /// <summary>
    /// 드레인 설정 변경
    /// </summary>
    public void SetDrainSettings(float newDrainAmount, float newDrainInterval)
    {
        drainAmount = newDrainAmount;
        drainInterval = newDrainInterval;
        Debug.Log($"Danger increase settings updated: {drainAmount} every {drainInterval} seconds");
    }
    
    /// <summary>
    /// 드레인 활성화/비활성화
    /// </summary>
    public void SetDrainEnabled(bool enabled)
    {
        enableDrain = enabled;
        if (!enabled)
        {
            StopDrain();
        }
        Debug.Log($"Danger drain {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// 유예 시간 설정
    /// </summary>
    public void SetGracePeriod(float newGracePeriod)
    {
        gracePeriod = newGracePeriod;
        Debug.Log($"Grace period set to {gracePeriod} seconds");
    }
    
    /// <summary>
    /// 현재 드레인 상태 정보
    /// </summary>
    public string GetDrainInfo()
    {
        return $"Draining: {isDraining}, Time outside: {timeSinceLeftSafeZone:F1}s, Grace period: {gracePeriod}s";
    }
}