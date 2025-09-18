using UnityEngine;

/// <summary>
/// 안전지대 밖에서 지속적인 HP 감소를 처리하는 시스템
/// </summary>
public class HPDrainSystem : MonoBehaviour
{
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
    private float drainTimer = 0f;
    
    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        playerStatus = GetComponent<PlayerStatus>();
        
        if (playerHealth == null)
        {
            Debug.LogError("HPDrainSystem requires a Health component!");
        }
        
        if (playerStatus == null)
        {
            Debug.LogError("HPDrainSystem requires a PlayerStatus component!");
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
        if (!enableDrain || playerStatus == null || playerHealth == null) return;
        
        // 안전지대에 있거나 죽었다면 드레인하지 않음
        if (playerStatus.IsInSafeZone || playerStatus.IsDead) return;
        
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