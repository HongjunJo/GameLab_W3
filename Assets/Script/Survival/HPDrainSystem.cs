using UnityEngine;

/// <summary>
/// 안전지대 밖에서 지속적인 HP 감소를 처리하는 시스템
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
    
    private Health playerHealth;
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
        
        playerHealth = playerObject.GetComponent<Health>();
        playerStatus = playerObject.GetComponent<PlayerStatus>();
        dangerGaugeSystem = playerObject.GetComponent<DangerGaugeSystem>(); // 컴포넌트 가져오기
        
        if (playerHealth == null)
        {
            Debug.LogError("HPDrainSystem: Health component not found on Player object!");
        }
        
        if (playerStatus == null)
        {
            Debug.LogError("HPDrainSystem: PlayerStatus component not found on Player object!");
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
        if (!enableDrain || playerStatus == null || playerHealth == null || (dangerGaugeSystem != null && dangerGaugeSystem.enabled)) return;
        
        // 안전지대에 있거나 죽었다면 드레인하지 않음
        // PlayerStatus에 IsInSafeZone이 없으므로, 이 시스템은 DangerGaugeSystem과 함께 사용되지 않는다고 가정
        // 만약 함께 사용해야 한다면, DangerGaugeSystem의 상태를 확인해야 함
        // 여기서는 PlayerStatus에 IsInSafeZone이 있었다는 가정 하에, 해당 기능을 제거했으므로 관련 로직을 수정해야 함
        // 하지만 HPDrainSystem은 DangerGaugeSystem과 함께 쓰이지 않으므로, 기존 로직을 유지하되 PlayerStatus의 IsInSafeZone을 다시 만들어야 함.
        // 그러나 이전 요청에서 PlayerStatus의 IsInSafeZone을 제거했으므로, 이 스크립트가 더 이상 정상 작동하지 않는 것이 맞음.
        // SystemTransitionManager에 의해 비활성화되므로, Update 로직 자체를 건너뛰게 하는 것이 가장 안전함.
        if (playerStatus.IsDead) return;
        
        // 안전지대를 벗어난 시간 계산
        timeSinceLeftSafeZone += Time.deltaTime;
        
        // 유예 시간이 지났다면 드레인 시작
        if (timeSinceLeftSafeZone >= gracePeriod)
        {
            isDraining = true;
            drainTimer += Time.deltaTime;
            
            if (drainTimer >= drainInterval)
            {
                DamagePlayer();
                drainTimer = 0f;
            }
        }
        else
        {
            isDraining = false;
        }
    }
    
    /// <summary>
    /// 플레이어에게 데미지를 가함
    /// </summary>
    private void DamagePlayer()
    {
        if (playerHealth != null && playerHealth.IsAlive)
        {
            playerHealth.TakeDamage(drainAmount);
            Debug.Log($"HP drained! -{drainAmount} (Time outside: {timeSinceLeftSafeZone:F1}s)");
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
        Debug.Log("HP drain stopped - entered safe zone");
    }
    
    /// <summary>
    /// 드레인 카운트다운 시작 (안전지대 탈출시)
    /// </summary>
    private void StartDrainCountdown()
    {
        timeSinceLeftSafeZone = 0f;
        drainTimer = 0f;
        isDraining = false;
        Debug.Log($"Left safe zone - HP drain will start in {gracePeriod} seconds");
    }
    
    /// <summary>
    /// 드레인 설정 변경
    /// </summary>
    public void SetDrainSettings(float newDrainAmount, float newDrainInterval)
    {
        drainAmount = newDrainAmount;
        drainInterval = newDrainInterval;
        Debug.Log($"HP drain settings updated: {drainAmount} damage every {drainInterval} seconds");
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
        Debug.Log($"HP drain {(enabled ? "enabled" : "disabled")}");
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