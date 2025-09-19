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
    [SerializeField] private float maxPower = 0f; // 초기값 0으로 변경
    [SerializeField] private float currentPower = 0f; // 초기값 0으로 변경
    
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
        
        // 초기에는 0/0으로 시작
        currentPower = 0f;
        maxPower = 0f;
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
        
        // 현재 전력은 변경하지 않음 (최대 용량만 증가)
        // 현재 전력이 새로운 최대값을 초과하지 않도록 클램프
        currentPower = Mathf.Min(currentPower, maxPower);
        
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log($"Power capacity upgraded! {oldMaxPower} -> {maxPower} (Current: {currentPower})");
    }
    
    /// <summary>
    /// 전력 생산 (발전기에서 호출)
    /// </summary>
    public void GeneratePower(float amount)
    {
        if (amount <= 0) return;
        
        float oldCurrent = currentPower;
        currentPower = Mathf.Min(currentPower + amount, maxPower);
        
        GameEvents.PowerChanged(currentPower, maxPower);
        Debug.Log($"Generated {amount} power. Current: {currentPower}/{maxPower} (was {oldCurrent})");
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
        // 현재 전력은 증가시키지 않음 (용량만 증가)
        currentPower = Mathf.Min(currentPower, maxPower);
        GameEvents.PowerChanged(currentPower, maxPower);
    }
}