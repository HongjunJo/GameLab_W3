using UnityEngine;

/// <summary>
/// 전력 시스템을 중앙에서 관리하는 싱글톤 매니저
/// </summary>
public class PowerManager : MonoBehaviour
{
    private static PowerManager _instance;
    public static PowerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PowerManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PowerManager");
                    _instance = go.AddComponent<PowerManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Power Settings")]
    [SerializeField] private float maxPower = 100f;
    [SerializeField] private float currentPower = 100f;
    
    [Header("Debug Info")]
    [SerializeField] private float powerUsageHistory = 0f;
    
    public float CurrentPower => currentPower;
    public float MaxPower => maxPower;
    public float PowerPercentage => maxPower > 0 ? currentPower / maxPower : 0f;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 시작시 최대 전력으로 설정
        currentPower = maxPower;
    }
    
    private void Start()
    {
        // 초기 이벤트 발생
        GameEvents.PowerChanged(currentPower, maxPower);
    }
    
    /// <summary>
    /// 전력 소모
    /// </summary>
    public bool SpendPower(float amount)
    {
        if (amount <= 0) return true;
        
        if (currentPower < amount)
        {
            Debug.LogWarning($"Not enough power! Required: {amount}, Current: {currentPower}");
            return false;
        }
        
        currentPower -= amount;
        powerUsageHistory += amount;
        
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log($"Spent {amount} power. Remaining: {currentPower}/{maxPower}");
        
        return true;
    }
    
    /// <summary>
    /// 최대 전력량 증가 (발전기 업그레이드)
    /// </summary>
    public void UpgradeMaxPower(float increaseAmount)
    {
        if (increaseAmount <= 0) return;
        
        float oldMaxPower = maxPower;
        maxPower += increaseAmount;
        
        // 현재 전력도 증가량만큼 추가 (업그레이드 보상)
        currentPower += increaseAmount;
        
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log($"Power upgraded! {oldMaxPower} -> {maxPower} (Current: {currentPower})");
    }
    
    /// <summary>
    /// 전력이 충분한지 확인
    /// </summary>
    public bool HasEnoughPower(float amount)
    {
        return currentPower >= amount;
    }
    
    /// <summary>
    /// 전력 회복 (미래 확장용 - 충전소 등)
    /// </summary>
    public void RestorePower(float amount)
    {
        if (amount <= 0) return;
        
        currentPower = Mathf.Min(currentPower + amount, maxPower);
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log($"Restored {amount} power. Current: {currentPower}/{maxPower}");
    }
    
    /// <summary>
    /// 전력 완전 충전
    /// </summary>
    public void FullRecharge()
    {
        currentPower = maxPower;
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log("Power fully recharged!");
    }
    
    /// <summary>
    /// 디버그용 - 최대 전력 설정
    /// </summary>
    [ContextMenu("Set Max Power to 250")]
    public void SetMaxPowerTest()
    {
        maxPower = 250f;
        currentPower = maxPower;
        GameEvents.PowerChanged(currentPower, maxPower);
    }
}