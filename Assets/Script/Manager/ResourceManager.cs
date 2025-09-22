using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 자원을 중앙에서 관리하는 싱글톤 매니저
/// </summary>
public class ResourceManager : MonoBehaviour
{
    private static ResourceManager _instance;
    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ResourceManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ResourceManager");
                    _instance = go.AddComponent<ResourceManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Debug - Current Resources")]
    [SerializeField] private List<ResourceDisplay> debugResources = new List<ResourceDisplay>();
    
    [Header("Debug - Test Resources")]
    [Tooltip("F8 키를 눌렀을 때 지급할 테스트 자원 목록입니다.")]
    [SerializeField] private List<ResourceCost> testResources = new List<ResourceCost>();

    private Dictionary<MineralData, int> resources = new Dictionary<MineralData, int>();
    
    [System.Serializable]
    public class ResourceDisplay
    {
        public MineralData mineral;
        public int amount;
    }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 게임 시작 시 초기 자원 상태를 UI에 알림
        GameEvents.ResourceChanged(new Dictionary<MineralData, int>(resources));
        Debug.Log("ResourceManager initialized - initial resources sent to UI");
    }

    private void Update()
    {
        // 디버그용: F8 키를 누르면 테스트 자원을 지급합니다.
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Debug.Log("[DEBUG] F8 키 입력 감지. 테스트 자원을 지급합니다.");
            foreach (var item in testResources)
            {
                if (item.mineral != null && item.amount > 0)
                {
                    AddResource(item.mineral, item.amount);
                }
            }
        }
    }
    
    /// <summary>
    /// 자원 추가
    /// </summary>
    public void AddResource(MineralData mineral, int amount)
    {
        if (mineral == null || amount <= 0) return;
        
        if (resources.ContainsKey(mineral))
        {
            resources[mineral] += amount;
        }
        else
        {
            resources[mineral] = amount;
        }
        
        UpdateDebugDisplay();
        GameEvents.ResourceAdded(mineral, amount);
        GameEvents.ResourceChanged(new Dictionary<MineralData, int>(resources));
        
        Debug.Log($"Added {amount} {mineral.mineralName}. Total: {resources[mineral]}");
    }
    
    /// <summary>
    /// 자원 사용
    /// </summary>
    public bool UseResource(MineralData mineral, int amount)
    {
        if (mineral == null || amount <= 0) return false;
        
        if (!HasEnoughResource(mineral, amount)) return false;
        
        resources[mineral] -= amount;
        // 한 번이라도 습득한 자원은 0이 되어도 딕셔너리에서 제거하지 않음
        if (resources[mineral] < 0)
        {
            resources[mineral] = 0; // 음수가 되지 않도록 안전장치
        }
        
        UpdateDebugDisplay();
        GameEvents.ResourceUsed(mineral, amount);
        GameEvents.ResourceChanged(new Dictionary<MineralData, int>(resources));
        
        Debug.Log($"Used {amount} {mineral.mineralName}. Remaining: {GetResourceAmount(mineral)}");
        return true;
    }
    
    /// <summary>
    /// 자원이 충분한지 확인
    /// </summary>
    public bool HasEnoughResource(MineralData mineral, int amount)
    {
        if (mineral == null || amount <= 0) return true;
        return resources.ContainsKey(mineral) && resources[mineral] >= amount;
    }
    
    /// <summary>
    /// 특정 자원의 보유량 확인
    /// </summary>
    public int GetResourceAmount(MineralData mineral)
    {
        if (mineral == null) return 0;
        return resources.ContainsKey(mineral) ? resources[mineral] : 0;
    }
    
    /// <summary>
    /// 모든 자원 정보 반환
    /// </summary>
    public Dictionary<MineralData, int> GetAllResources()
    {
        return new Dictionary<MineralData, int>(resources);
    }
    
    /// <summary>
    /// 디버그용 인스펙터 표시 업데이트
    /// </summary>
    private void UpdateDebugDisplay()
    {
        debugResources.Clear();
        foreach (var kvp in resources)
        {
            debugResources.Add(new ResourceDisplay { mineral = kvp.Key, amount = kvp.Value });
        }
    }
    
    /// <summary>
    /// 게임 시작시 초기 자원 설정 (테스트용)
    /// </summary>
    [ContextMenu("Add Test Resources")]
    public void AddTestResources()
    {
        // 테스트용 - 기본 자원들을 추가
        // 실제 게임에서는 레벨 디자인이나 세이브 데이터에서 로드
    }
}