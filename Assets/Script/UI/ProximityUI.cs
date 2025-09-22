using UnityEngine;

/// <summary>
/// 플레이어가 일정 범위 내에 들어오면 연결된 월드 스페이스 UI를 활성화하고,
/// 범위를 벗어나면 비활성화하는 스크립트.
/// </summary>
public class ProximityUI : MonoBehaviour
{
    [Header("UI 설정")]
    [Tooltip("플레이어가 가까이 왔을 때 활성화할 UI 오브젝트입니다.")]
    [SerializeField] private GameObject uiToShow;

    [Header("감지 설정")]
    [Tooltip("UI를 활성화할 감지 범위입니다.")]
    [SerializeField] private float detectionRadius = 5f;
    [Tooltip("플레이어를 식별하기 위한 태그입니다.")]
    [SerializeField] private string playerTag = "Player";

    private Transform playerTransform;
    private bool isPlayerInRange = false;

    private void Start()
    {
        // 플레이어 오브젝트를 찾습니다.
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError($"'{playerTag}' 태그를 가진 플레이어를 찾을 수 없습니다. 플레이어 태그를 확인해주세요.", this);
            enabled = false; // 플레이어를 못찾으면 스크립트 비활성화
            return;
        }

        // 시작 시 UI를 숨깁니다.
        if (uiToShow != null)
        {
            uiToShow.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerTransform == null || uiToShow == null) return;

        // 플레이어와 이 오브젝트 사이의 거리를 계산합니다.
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 플레이어가 범위 안에 있는지 확인합니다.
        bool shouldBeActive = distance <= detectionRadius;

        // UI 상태가 변경되어야 할 때만 SetActive를 호출하여 최적화합니다.
        if (shouldBeActive != isPlayerInRange)
        {
            isPlayerInRange = shouldBeActive;
            uiToShow.SetActive(isPlayerInRange);
        }
    }
}