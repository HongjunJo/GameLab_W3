using UnityEngine;
using System.Collections;

/// <summary>
/// 광산 - 특정 광물을 주기적으로 생산하는 건물
/// </summary>
public class Mine : MonoBehaviour, IInteractable
{
    [Header("Mine Settings")]
    [SerializeField] private BuildingRecipe activationRecipe;
    [SerializeField] private bool isActive = false;
    [SerializeField] private bool isBuilt = false;
    
    [Header("Production")]
    [SerializeField] private MineralData producedMineral;
    [SerializeField] private int productionAmount = 1;
    [SerializeField] private float productionTime = 10f;
    [SerializeField] private int currentStock = 0;
    [SerializeField] private int maxStock = 10;
    
    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material productionMaterial;
    
    [Header("Debug")]
    [SerializeField] private float productionTimer = 0f;
    
    private Coroutine productionCoroutine;
    
    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        // 레시피에서 생산 정보 가져오기
        if (activationRecipe != null)
        {
            if (activationRecipe.producedMineral != null)
            {
                producedMineral = activationRecipe.producedMineral;
            }
            productionAmount = activationRecipe.productionAmount;
            productionTime = activationRecipe.productionTime;
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
        if (!isBuilt)
        {
            // 건설 가능한지 확인
            return activationRecipe != null && activationRecipe.CanAfford();
        }
        else if (isActive)
        {
            // 생산된 자원 수집 가능한지 확인
            return currentStock > 0;
        }
        
        return false;
    }
    
    public void Interact()
    {
        if (!isBuilt && CanInteract())
        {
            BuildMine();
        }
        else if (isActive && currentStock > 0)
        {
            CollectResources();
        }
    }
    
    public string GetInteractionText()
    {
        if (!isBuilt)
        {
            if (activationRecipe != null && activationRecipe.CanAfford())
            {
                return $"Build Mine (Produces {producedMineral?.mineralName})";
            }
            else
            {
                return "Need more resources to build";
            }
        }
        else if (isActive && currentStock > 0)
        {
            return $"Collect {producedMineral?.mineralName} x{currentStock}";
        }
        else if (isActive)
        {
            return "Mine is producing...";
        }
        
        return "Inactive Mine";
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
            isActive = true;
            
            // 생산 시작
            StartProduction();
            
            // 비주얼 업데이트
            UpdateVisual();
            
            // 이벤트 발생
            GameEvents.BuildingActivated($"Mine_{producedMineral?.mineralName}");
            
            Debug.Log($"Mine built! Now producing {producedMineral?.mineralName}");
        }
    }
    
    /// <summary>
    /// 생산 시작
    /// </summary>
    private void StartProduction()
    {
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
        }
        
        productionCoroutine = StartCoroutine(ProductionLoop());
    }
    
    /// <summary>
    /// 생산 중지
    /// </summary>
    private void StopProduction()
    {
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
            productionCoroutine = null;
        }
    }
    
    /// <summary>
    /// 생산 루프 코루틴
    /// </summary>
    private IEnumerator ProductionLoop()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(productionTime);
            
            if (currentStock < maxStock)
            {
                ProduceResource();
            }
        }
    }
    
    /// <summary>
    /// 자원 생산
    /// </summary>
    private void ProduceResource()
    {
        if (producedMineral == null) return;
        
        currentStock += productionAmount;
        currentStock = Mathf.Min(currentStock, maxStock);
        
        // 이벤트 발생
        GameEvents.MineralProduced(producedMineral, productionAmount);
        
        Debug.Log($"Mine produced {productionAmount} {producedMineral.mineralName}. Stock: {currentStock}/{maxStock}");
    }
    
    /// <summary>
    /// 생산된 자원 수집
    /// </summary>
    private void CollectResources()
    {
        if (currentStock <= 0 || producedMineral == null) return;
        
        // ResourceManager에 자원 추가
        ResourceManager.Instance.AddResource(producedMineral, currentStock);
        
        Debug.Log($"Collected {currentStock} {producedMineral.mineralName} from mine");
        
        // 재고 초기화
        currentStock = 0;
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (objectRenderer == null) return;
        
        if (isActive && activeMaterial != null)
        {
            objectRenderer.material = activeMaterial;
        }
        else if (isBuilt && productionMaterial != null && currentStock > 0)
        {
            objectRenderer.material = productionMaterial;
        }
        else if (inactiveMaterial != null)
        {
            objectRenderer.material = inactiveMaterial;
        }
    }
    
    /// <summary>
    /// 광산 활성화 상태 설정
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (isActive && isBuilt)
        {
            StartProduction();
        }
        else
        {
            StopProduction();
        }
        
        UpdateVisual();
    }
    
    /// <summary>
    /// 수동으로 광산 설정 (테스트용)
    /// </summary>
    [ContextMenu("Build Mine (Test)")]
    public void BuildMineTest()
    {
        isBuilt = true;
        isActive = true;
        StartProduction();
        UpdateVisual();
    }
    
    private void OnDestroy()
    {
        StopProduction();
    }
}