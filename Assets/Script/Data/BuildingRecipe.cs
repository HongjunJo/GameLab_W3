using System.Collections.Generic;
using UnityEngine;
using System.Text;

[System.Serializable]
public class ResourceCost
{
    public MineralData mineral;
    public int amount;
    
    public ResourceCost(MineralData mineral, int amount)
    {
        this.mineral = mineral;
        this.amount = amount;
    }
}

[CreateAssetMenu(fileName = "New Building Recipe", menuName = "Game Data/Building Recipe")]
public class BuildingRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Costs")]
    public List<ResourceCost> resourceCosts = new List<ResourceCost>();
    public float electricityCost;
    
    [Header("Production (for mines only)")]
    public MineralData producedMineral;
    public int productionAmount = 1;
    public float productionTime = 10f;
    
    [Header("Power Upgrade (for generators only)")]
    public float powerIncrease;
    
    /// <summary>
    /// 레시피의 비용을 만족하는지 확인
    /// </summary>
    public bool CanAfford()
    {
        // 전력 체크
        if (PowerManager.Instance.CurrentPower < electricityCost)
            return false;
            
        // 자원 체크
        foreach (var cost in resourceCosts)
        {
            if (!ResourceManager.Instance.HasEnoughResource(cost.mineral, cost.amount))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 레시피의 비용을 소모
    /// </summary>
    public bool ConsumeCost()
    {
        if (!CanAfford()) return false;
        
        // 전력 소모
        PowerManager.Instance.SpendPower(electricityCost);
        
        // 자원 소모
        foreach (var cost in resourceCosts)
        {
            ResourceManager.Instance.UseResource(cost.mineral, cost.amount);
        }
        
        return true;
    }

    /// <summary>
    /// 이 레시피에 필요한 자원 비용을 문자열로 반환합니다.
    /// 예: "Iron: 20/50, Gold: 5/10"
    /// </summary>
    /// <returns>자원 비용 정보 문자열</returns>
    public string GetCostAsString()
    {
        if (resourceCosts == null || resourceCosts.Count == 0)
        {
            if (electricityCost > 0)
            {
                return $"Power: {PowerManager.Instance.CurrentPower:F0}/{electricityCost:F0}";
            }
            return "No resource cost";
        }

        StringBuilder sb = new StringBuilder();
        foreach (var cost in resourceCosts)
        {
            int currentAmount = ResourceManager.Instance.GetResourceAmount(cost.mineral);
            if (sb.Length > 0) sb.Append(", ");
            sb.Append($"{cost.mineral.mineralName}: {currentAmount}/{cost.amount}");
        }
        if (electricityCost > 0)
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append($"Power: {PowerManager.Instance.CurrentPower:F0}/{electricityCost:F0}");
        }
        return sb.ToString();
    }
}