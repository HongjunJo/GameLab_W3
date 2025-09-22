using UnityEngine;

/// <summary>
/// 플레이어의 리스폰 위치를 지정하는 구역(Sector)을 정의합니다.
/// 이 구역 안에서 사망하면 지정된 리스폰 포인트에서 부활합니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RespawnSector : MonoBehaviour
{
    [Header("Sector Settings")]
    [Tooltip("이 섹터에서 사망 시 리스폰될 위치입니다.")]
    [SerializeField] private Transform respawnPoint;

    [Tooltip("이 섹터의 이름입니다. (디버그용)")]
    [SerializeField] private string sectorName = "Sector";

    [Tooltip("이 섹터가 메인 섹터인지 여부입니다. 메인 섹터에 진입 시 임시 자원이 저장됩니다.")]
    public bool isMainSector = false;

    [Header("Visuals")]
    [SerializeField] private Color gizmoColor = new Color(0f, 0.5f, 1f, 0.3f); // 파란색 계열


    public Transform RespawnPoint => respawnPoint;
    public string SectorName => sectorName;

    private void Awake()
    {
        // 콜라이더가 반드시 트리거여야 합니다.
        var col = GetComponent<BoxCollider2D>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"RespawnSector '{name}'의 BoxCollider2D가 트리거가 아니므로 자동으로 설정합니다.");
        }

        if (respawnPoint == null)
        {
            Debug.LogError($"RespawnSector '{name}'에 리스폰 포인트가 지정되지 않았습니다!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus status = other.GetComponent<PlayerStatus>();
            if (status != null)
            {
                // 플레이어가 섹터에 진입하면 현재 섹터 정보를 갱신합니다.
                status.UpdateCurrentSector(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStatus status = other.GetComponent<PlayerStatus>();
            // 플레이어가 현재 이 섹터를 자신의 CurrentSector로 등록해 놓은 경우에만 초기화합니다.
            if (status != null && status.CurrentSector == this)
            {
                status.ClearCurrentSector();
            }
        }
    }

    private void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
