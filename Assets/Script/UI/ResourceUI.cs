using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 자원 UI를 표시하고 관리하는 컴포넌트
/// </summary>
public class ResourceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourceItemPrefab;
    [SerializeField] private Text totalResourcesText;
    
    [Header("Settings")]
    [SerializeField] private bool showOnlyOwnedResources = true;
    [SerializeField] private bool autoUpdate = true;
    
    private Dictionary<MineralData, ResourceUIItem> resourceItems = new Dictionary<MineralData, ResourceUIItem>();
    
    [System.Serializable]
    public class ResourceUIItem
    {
        public GameObject gameObject;
        public Image icon;
        public Text nameText;
        public Text amountText;
        public MineralData mineralData;
    }
    
    private void OnEnable()
    {
        if (autoUpdate)
        {
            GameEvents.OnResourceChanged += UpdateResourceDisplay;
        }
    }
    
    private void OnDisable()
    {
        GameEvents.OnResourceChanged -= UpdateResourceDisplay;
    }
    
    private void Start()
    {
        // 초기 UI 업데이트
        if (ResourceManager.Instance != null)
        {
            UpdateResourceDisplay(ResourceManager.Instance.GetAllResources());
        }
    }
    
    /// <summary>
    /// 자원 표시 업데이트
    /// </summary>
    private void UpdateResourceDisplay(Dictionary<MineralData, int> resources)
    {
        if (resourceContainer == null) return;
        
        // 현재 소유한 자원들을 표시
        foreach (var kvp in resources)
        {
            MineralData mineral = kvp.Key;
            int amount = kvp.Value;
            
            if (mineral == null) continue;
            
            // 자원 항목이 없다면 생성
            if (!resourceItems.ContainsKey(mineral))
            {
                CreateResourceItem(mineral);
            }
            
            // 자원 항목 업데이트
            if (resourceItems.ContainsKey(mineral))
            {
                UpdateResourceItem(mineral, amount);
            }
        }
        
        // 더 이상 소유하지 않은 자원들 제거 (옵션에 따라)
        if (showOnlyOwnedResources)
        {
            var keysToRemove = resourceItems.Keys.Where(mineral => 
                !resources.ContainsKey(mineral) || resources[mineral] <= 0).ToList();
            
            foreach (var mineral in keysToRemove)
            {
                RemoveResourceItem(mineral);
            }
        }
        
        // 총 자원 수 업데이트
        UpdateTotalResourcesText(resources);
    }
    
    /// <summary>
    /// 새로운 자원 UI 항목 생성
    /// </summary>
    private void CreateResourceItem(MineralData mineral)
    {
        if (resourceItemPrefab == null || resourceContainer == null) return;
        
        GameObject itemObj = Instantiate(resourceItemPrefab, resourceContainer);
        ResourceUIItem item = new ResourceUIItem
        {
            gameObject = itemObj,
            mineralData = mineral
        };
        
        // UI 컴포넌트 찾기
        item.icon = itemObj.transform.Find("Icon")?.GetComponent<Image>();
        item.nameText = itemObj.transform.Find("Name")?.GetComponent<Text>();
        item.amountText = itemObj.transform.Find("Amount")?.GetComponent<Text>();
        
        // 아이콘 설정
        if (item.icon != null && mineral.icon != null)
        {
            item.icon.sprite = mineral.icon;
            item.icon.color = mineral.mineralColor;
        }
        
        // 이름 설정
        if (item.nameText != null)
        {
            item.nameText.text = mineral.mineralName;
        }
        
        resourceItems[mineral] = item;
    }
    
    /// <summary>
    /// 자원 UI 항목 업데이트
    /// </summary>
    private void UpdateResourceItem(MineralData mineral, int amount)
    {
        if (!resourceItems.ContainsKey(mineral)) return;
        
        ResourceUIItem item = resourceItems[mineral];
        if (item.amountText != null)
        {
            item.amountText.text = amount.ToString();
        }
        
        // 양이 0인 경우 반투명하게
        if (item.gameObject != null)
        {
            CanvasGroup canvasGroup = item.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = item.gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = amount > 0 ? 1f : 0.5f;
        }
    }
    
    /// <summary>
    /// 자원 UI 항목 제거
    /// </summary>
    private void RemoveResourceItem(MineralData mineral)
    {
        if (!resourceItems.ContainsKey(mineral)) return;
        
        ResourceUIItem item = resourceItems[mineral];
        if (item.gameObject != null)
        {
            Destroy(item.gameObject);
        }
        
        resourceItems.Remove(mineral);
    }
    
    /// <summary>
    /// 총 자원 수 텍스트 업데이트
    /// </summary>
    private void UpdateTotalResourcesText(Dictionary<MineralData, int> resources)
    {
        if (totalResourcesText == null) return;
        
        int totalTypes = resources.Count;
        int totalAmount = resources.Values.Sum();
        
        totalResourcesText.text = $"Resources: {totalTypes} types, {totalAmount} total";
    }
    
    /// <summary>
    /// 수동으로 UI 업데이트
    /// </summary>
    [ContextMenu("Update Resource Display")]
    public void ForceUpdateDisplay()
    {
        if (ResourceManager.Instance != null)
        {
            UpdateResourceDisplay(ResourceManager.Instance.GetAllResources());
        }
    }
    
    /// <summary>
    /// 모든 자원 항목 제거
    /// </summary>
    public void ClearAllItems()
    {
        foreach (var item in resourceItems.Values)
        {
            if (item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        resourceItems.Clear();
    }
    
    /// <summary>
    /// 특정 자원의 양 가져오기 (UI에서 표시용)
    /// </summary>
    public int GetDisplayedResourceAmount(MineralData mineral)
    {
        if (ResourceManager.Instance != null)
        {
            return ResourceManager.Instance.GetResourceAmount(mineral);
        }
        return 0;
    }
}