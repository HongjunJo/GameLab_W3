using UnityEngine;
using System.Collections;

/// <summary>
/// 광산 - 특정 광물을 주기적으로 생산하는 건물
/// </summary>
public class Mine : MonoBehaviour, IInteractable
{
    [Header("Mine Settings")]
    [SerializeField] private BuildingRecipe activationRecipe;
    [SerializeField] private bool isBuilt = false;
    
    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;
    
    // Public 속성들
    public bool IsBuilt => isBuilt;
    
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
    
    public void StopInteract()
    {
        // 광산은 홀드 상호작용이 없으므로 비워둡니다.
    }

    public bool CanInteract()
    {
        // 건설되지 않았고, 건설 비용이 충분할 때만 상호작용 가능
        return !isBuilt && activationRecipe != null && activationRecipe.CanAfford();
    }
    
    public void Interact()
    {
        if (CanInteract())
        {
            BuildMine();
        }
    }
    
    public string GetInteractionText()
    {
        // 이미 건설되었다면 상호작용 텍스트를 표시하지 않음
        if (!isBuilt)
        {
            string costText = activationRecipe.GetCostAsString();
            if (CanInteract())
            {
                return $"Build {activationRecipe.recipeName}\n{costText}";
            }
            else
            {
                return $"Build {activationRecipe.recipeName}\n<color=red>Need more resources</color>\n{costText}";
            }
        }
        return $"{activationRecipe.recipeName} (Built)\nPress 'W' to Teleport";
    }
    
    /// <summary>
    /// 광산 건설
    /// </summary>
    private void BuildMine()
    {
        if (activationRecipe == null || !activationRecipe.CanAfford()) return;
        
        // 비용 소모
        if (activationRecipe.ConsumeCost())
        {
            isBuilt = true;
            
            // 비주얼 업데이트
            UpdateVisual();
            
            // 이벤트 발생
            GameEvents.BuildingActivated($"Mine_{activationRecipe.recipeName}");
            
            Debug.Log($"Mine built!");
        }
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (objectRenderer == null) return;
        
        if (isBuilt && activeMaterial != null)
        {
            objectRenderer.material = activeMaterial;
        }
        else if (inactiveMaterial != null)
        {
            objectRenderer.material = inactiveMaterial;
        }
    }
    
    /// <summary>
    /// 수동으로 광산 설정 (테스트용)
    /// </summary>
    [ContextMenu("Build Mine (Test)")]
    public void BuildMineTest()
    {
        isBuilt = true;
        UpdateVisual();
    }
}