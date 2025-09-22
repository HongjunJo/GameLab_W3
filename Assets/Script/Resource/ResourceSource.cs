using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

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
    [Tooltip("채집 시 비활성화될 시각적 표현을 담당하는 자식 오브젝트")]
    [SerializeField] private GameObject visualObject;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private string interactionText = "Collect";
    
    // 홀드 상호작용 관련 변수
    private bool isInteracting = false;
    private float currentHoldTime = 0f;
    private bool isDepleted = false;
    private WorldspaceProgressUI progressUI;
    private int currentHp;
    
    private void Awake()
    {
        // 랜덤 양 설정
        if (isRandomAmount)
        {
            amount = Random.Range(minAmount, maxAmount + 1);
        }

        if (mineralData != null)
        {
            currentHp = mineralData.maxHp;
        }
        
        // 오브젝트 렌더러 자동 할당
        if (visualObject == null && transform.childCount > 0)
        {
            visualObject = transform.GetChild(0).gameObject;
        }
        
        // 광물 데이터에서 머티리얼 색상 설정 (추후 확장용)

        // 자식 오브젝트에서 진행률 UI를 찾습니다.
        progressUI = GetComponentInChildren<WorldspaceProgressUI>();
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
        return !isDepleted && mineralData != null;
    }
    
    // 상호작용 시작 (키를 누르기 시작)
    public void Interact() 
    {
        if (!CanInteract()) return;
        isInteracting = true;
        currentHoldTime = 0f; // 홀드 시간 초기화
        if (progressUI != null)
        {
            progressUI.Show();
            progressUI.UpdateProgress(0);
        }
        Debug.Log($"{mineralData.mineralName} 채집 시작...");
    }
    
    // 상호작용 중단 (키를 떼거나 범위 이탈)
    public void StopInteract()
    {
        if (isInteracting)
        {
            isInteracting = false;
            currentHoldTime = 0f;
            if (progressUI != null)
            {
                progressUI.Hide();
            }
            Debug.Log($"{mineralData.mineralName} 채집 중단.");
        }
    }
    
    private void Update()
    {
        // 상호작용 중일 때 홀드 시간 업데이트
        if (isInteracting)
        {
            currentHoldTime += Time.deltaTime;

            // 진행률 UI 업데이트
            if (progressUI != null)
            {
                float progress = Mathf.Clamp01(currentHoldTime / mineralData.miningDuration);
                progressUI.UpdateProgress(progress);
            }

            if (currentHoldTime >= mineralData.miningDuration)
            {
                CollectResource();
                isInteracting = false; // 채집 완료 후 상호작용 상태 해제
            }
        }
    }

    public string GetInteractionText()
    {
        if (!CanInteract()) return "";
        
        // 홀드 진행 시간을 표시
        float remainingTime = Mathf.Max(0, mineralData.miningDuration - currentHoldTime);
        return $"Hold to {interactionText} {mineralData.mineralName} ({remainingTime:F1}s)";
    }
    
    /// <summary>
    /// 자원 채집 실행
    /// </summary>
    private void CollectResource()
    {
        if (isDepleted) return;
        isDepleted = true;
        
        // 플레이어의 임시 인벤토리를 찾습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        TemporaryInventory tempInventory = player?.GetComponent<TemporaryInventory>();

        if (tempInventory != null)
        {
            // 임시 인벤토리에 자원과 출처(자기 자신)를 함께 추가합니다.
            tempInventory.AddResource(mineralData, amount, this);
        }
        else
        {
            Debug.LogError("플레이어에 TemporaryInventory 컴포넌트가 없습니다! 자원을 임시 저장할 수 없습니다.");
        }
        
        if (progressUI != null)
        {
            progressUI.Hide();
        }

        Debug.Log($"채집 완료! {mineralData.mineralName} {amount}개를 획득했습니다.");
        // 오브젝트를 파괴하는 대신 재생성 코루틴을 시작합니다.
        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// 자원 재생성 코루틴
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        if (mineralData.respawnTime > 0f)
        {
            // 시각적 오브젝트와 콜라이더를 비활성화합니다.
            if (visualObject != null) visualObject.SetActive(false);
            GetComponent<Collider2D>().enabled = false; // 상호작용을 막기 위해 콜라이더도 끔

            Debug.Log($"{mineralData.mineralName}이(가) {mineralData.respawnTime}초 후 재생성됩니다.");

            // 설정된 시간만큼 대기
            yield return new WaitForSeconds(mineralData.respawnTime);

            ResetResource();
        }
        
        else
        {
            // 재생성 시간이 0 이하이면 오브젝트를 완전히 비활성화
            Debug.Log($"{mineralData.mineralName}이(가) 재생성되지 않고 비활성화됩니다.");
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 자원 상태를 초기화하고 다시 채집 가능하게 만듭니다.
    /// </summary>
    private void ResetResource()
    {
        isDepleted = false;
        currentHp = mineralData.maxHp;
        currentHoldTime = 0f;
        if (visualObject != null) visualObject.SetActive(true);
        GetComponent<Collider2D>().enabled = true;

        Debug.Log($"{mineralData.mineralName} 재생성 완료!");
    }

    /// <summary>
    /// 외부에서 자원을 강제로 리스폰시킬 때 호출됩니다 (예: 플레이어 사망).
    /// </summary>
    public void ForceRespawn()
    {
        // 오브젝트가 비활성화된 상태일 수 있으므로, 먼저 활성화합니다.
        // respawnTime이 0이어서 비활성화된 경우를 처리하기 위함입니다.
        gameObject.SetActive(true);

        StopAllCoroutines(); // 진행 중인 리스폰 코루틴이 있다면 중지
        ResetResource();
    }

    /// <summary>
    /// 자원 데이터 설정 (런타임에서 동적 생성시 사용)
    /// </summary>
    public void SetResourceData(MineralData mineral, int resourceAmount)
    {
        mineralData = mineral;
        amount = resourceAmount;
        currentHp = mineralData.maxHp;
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
        
        // 기본 콜라이더 추가
        BoxCollider col = resourceObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        
        return resourceObj;
    }
}