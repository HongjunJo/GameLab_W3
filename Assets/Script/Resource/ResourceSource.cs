using UnityEngine;

/// <summary>
/// 필드에 떨어져 있는 채집 가능한 자원
/// </summary>
public class ResourceSource : MonoBehaviour, IInteractable
{
    [Header("Resource Settings")]
    [SerializeField] private MineralData mineralData;
    [SerializeField] private int amount = 1;
    [SerializeField] private bool isRandomAmount = false;
    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 3;
    
    [Header("Visual Settings")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material defaultMaterial;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private string interactionText = "Collect";
    
    private bool hasBeenCollected = false;
    
    private void Awake()
    {
        // 랜덤 양 설정
        if (isRandomAmount)
        {
            amount = Random.Range(minAmount, maxAmount + 1);
        }
        
        // 오브젝트 렌더러 자동 할당
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        // 광물 데이터에서 머티리얼 색상 설정 (추후 확장용)
        UpdateVisual();
    }
    
    private void Start()
    {
        // 콜라이더 설정 확인 (상호작용을 위해)
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
    }
    
    public bool CanInteract()
    {
        return !hasBeenCollected && mineralData != null;
    }
    
    public void Interact()
    {
        if (!CanInteract()) return;
        
        CollectResource();
    }
    
    public string GetInteractionText()
    {
        if (!CanInteract()) return "";
        
        return $"{interactionText} {mineralData.mineralName} x{amount}";
    }
    
    /// <summary>
    /// 자원 채집 실행
    /// </summary>
    private void CollectResource()
    {
        if (hasBeenCollected) return;
        
        hasBeenCollected = true;
        
        // ResourceManager에 자원 추가
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResource(mineralData, amount);
        }
        
        Debug.Log($"Collected {amount} {mineralData.mineralName}");
        
        // 오브젝트 제거
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 자원 데이터 설정 (런타임에서 동적 생성시 사용)
    /// </summary>
    public void SetResourceData(MineralData mineral, int resourceAmount)
    {
        mineralData = mineral;
        amount = resourceAmount;
        
        // 비주얼 업데이트
        UpdateVisual();
    }
    
    /// <summary>
    /// 비주얼 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        // 현재는 메테리얼 기반이지만, 나중에 광물별 머티리얼을 설정할 수 있음
        if (objectRenderer != null && defaultMaterial != null)
        {
            objectRenderer.material = defaultMaterial;
        }
    }
    
    /// <summary>
    /// 상호작용 범위 표시 (디버그용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    /// <summary>
    /// 자원 생성 유틸리티 메서드 (정적)
    /// </summary>
    public static GameObject CreateResourceSource(MineralData mineralData, int amount, Vector3 position)
    {
        GameObject resourceObj = new GameObject($"Resource_{mineralData.mineralName}");
        resourceObj.transform.position = position;
        
        ResourceSource resource = resourceObj.AddComponent<ResourceSource>();
        resource.SetResourceData(mineralData, amount);
        
        // 기본 렌더러 추가 (Cube 기본형)
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(resourceObj.transform);
        cube.transform.localPosition = Vector3.zero;
        
        // 렌더러 연결
        resource.objectRenderer = cube.GetComponent<Renderer>();
        
        // 기본 콜라이더 추가
        BoxCollider col = resourceObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        
        return resourceObj;
    }
}