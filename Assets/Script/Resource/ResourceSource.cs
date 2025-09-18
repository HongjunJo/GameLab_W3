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
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    
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
        
        // 스프라이트 렌더러 자동 할당
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // 광물 데이터에서 스프라이트 설정
        if (mineralData != null && spriteRenderer != null && mineralData.icon != null)
        {
            spriteRenderer.sprite = mineralData.icon;
            spriteRenderer.color = mineralData.mineralColor;
        }
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
        
        // 채집 이펙트 생성
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // 채집 사운드 재생
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
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
        
        // 스프라이트 업데이트
        if (mineralData != null && spriteRenderer != null && mineralData.icon != null)
        {
            spriteRenderer.sprite = mineralData.icon;
            spriteRenderer.color = mineralData.mineralColor;
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
        
        // 기본 스프라이트 렌더러 추가
        SpriteRenderer sr = resourceObj.AddComponent<SpriteRenderer>();
        resource.spriteRenderer = sr;
        
        // 기본 콜라이더 추가
        CircleCollider2D col = resourceObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        
        return resourceObj;
    }
}