using UnityEngine;

/// <summary>
/// 발전기 - 최대 전력량을 증가시키는 건물
/// </summary>
public class PowerGenerator : MonoBehaviour, IInteractable
{
    [Header("Generator Settings")]
    [SerializeField] private BuildingRecipe[] upgradeRecipes;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private bool isBuilt = true; // 기본 발전기는 이미 건설된 상태
    
    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material[] levelMaterials;
    
    [Header("Debug Info")]
    [SerializeField] private float totalPowerGenerated = 0f;
    
    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxLevel;
    public bool IsMaxLevel => currentLevel >= maxLevel;
    
    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        UpdateVisual();
    }
    
    private void Start()
    {
        // 콜라이더 설정
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
        }
    }
    
    public bool CanInteract()
    {
        if (!isBuilt) return false;
        if (IsMaxLevel) return false;
        
        // 다음 레벨 업그레이드가 가능한지 확인
        BuildingRecipe nextRecipe = GetNextUpgradeRecipe();
        return nextRecipe != null && nextRecipe.CanAfford();
    }
    
    public void Interact()
    {
        if (CanInteract())
        {
            UpgradeGenerator();
        }
    }
    
    public void StopInteract()
    {
        // 발전기는 홀드 상호작용이 없으므로 비워둡니다.
    }

    public string GetInteractionText()
    {
        if (!isBuilt)
        {
            return "Generator not built";
        }
        
        if (IsMaxLevel)
        {
            return $"Generator (Max Level)";
        }
        
        BuildingRecipe nextRecipe = GetNextUpgradeRecipe();
        if (nextRecipe != null)
        {
            string costText = nextRecipe.GetCostAsString();
            if (CanInteract())
            {
                return $"Upgrade Generator (Lv.{currentLevel + 1})\n{costText}";
            }
            else
            {
                return $"Upgrade Generator (Lv.{currentLevel + 1})\n<color=red>Need more resources</color>\n{costText}";
            }
        }
        
        return $"Generator (Lv.{currentLevel})";
    }
    
    /// <summary>
    /// 발전기 업그레이드
    /// </summary>
    private void UpgradeGenerator()
    {
        BuildingRecipe recipe = GetNextUpgradeRecipe();
        if (recipe == null || !recipe.CanAfford()) return;
        
        // 비용 소모
        if (recipe.ConsumeCost())
        {
            currentLevel++;
            
            PowerManager.Instance.UpgradeMaxPower(recipe.powerIncrease);
            
            // 즉시 전력 생산 (업그레이드 보상)
            PowerManager.Instance.GeneratePower(recipe.powerIncrease);
            // 총 발전량 기록 (통계용)
            totalPowerGenerated += recipe.powerIncrease;
            
            // 비주얼 업데이트
            UpdateVisual();
            
            // 이벤트 발생
            GameEvents.BuildingActivated($"PowerGenerator_Lv{currentLevel}");
            
            Debug.Log($"Generator upgraded to level {currentLevel}! Max power increased by {recipe.powerIncrease}");
        }
    }
    
    /// <summary>
    /// 다음 업그레이드 레시피 가져오기
    /// </summary>
    private BuildingRecipe GetNextUpgradeRecipe()
    {
        if (upgradeRecipes == null || currentLevel >= upgradeRecipes.Length) return null;
        return upgradeRecipes[currentLevel];
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (objectRenderer == null || levelMaterials == null) return;
        
        int materialIndex = Mathf.Clamp(currentLevel, 0, levelMaterials.Length - 1);
        if (materialIndex < levelMaterials.Length && levelMaterials[materialIndex] != null)
        {
            objectRenderer.material = levelMaterials[materialIndex];
        }
    }
    
    /// <summary>
    /// 레벨 설정 (테스트용)
    /// </summary>
    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, maxLevel);
        UpdateVisual();
    }
    
    /// <summary>
    /// 현재 레벨의 업그레이드 정보 가져오기
    /// </summary>
    public BuildingRecipe GetCurrentLevelRecipe()
    {
        if (currentLevel == 0) return null;
        return upgradeRecipes != null && currentLevel - 1 < upgradeRecipes.Length ? 
               upgradeRecipes[currentLevel - 1] : null;
    }
    
    /// <summary>
    /// 다음 레벨 업그레이드 정보 가져오기
    /// </summary>
    public string GetUpgradeInfo()
    {
        if (IsMaxLevel) return "Max Level Reached";
        
        BuildingRecipe nextRecipe = GetNextUpgradeRecipe();
        if (nextRecipe == null) return "No upgrade available";
        
        return $"Next: +{nextRecipe.powerIncrease} Power";
    }
    
    /// <summary>
    /// 테스트용 업그레이드
    /// </summary>
    [ContextMenu("Upgrade Generator (Test)")]
    public void UpgradeGeneratorTest()
    {
        if (!IsMaxLevel)
        {
            currentLevel++;
            UpdateVisual();
            
            // 테스트용으로 50씩 증가 (용량 + 즉시 생산)
            PowerManager.Instance.UpgradeMaxPower(50f);
            PowerManager.Instance.GeneratePower(50f);
            totalPowerGenerated += 50f;
        }
    }
}