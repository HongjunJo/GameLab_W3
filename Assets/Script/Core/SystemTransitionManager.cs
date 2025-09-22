using UnityEngine;

/// <summary>
/// 기존 HPDrainSystem을 비활성화하고 DangerGaugeSystem으로 교체하는 매니저
/// </summary>
public class SystemTransitionManager : MonoBehaviour // HP 시스템 관련 로직 제거
{
    [Header("System Settings")]
    [SerializeField] private bool useDangerGaugeSystem = true;
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSystems();
        }
    }
    
    /// <summary>
    /// 시스템 자동 설정
    /// </summary>
    public void SetupSystems()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player not found! Cannot setup systems.");
            return;
        }
        
        // 시스템 전환 순서 중요: DangerGaugeSystem을 먼저 추가
        if (useDangerGaugeSystem)
        {
            SetupDangerGaugeSystem(player);
        }
        
        // PlayerStatus 컴포넌트 체크 강제 (DangerGaugeSystem 추가 후)
        var playerStatus = player.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            // 컴포넌트 다시 체크하도록 강제
            playerStatus.RefreshComponents();
            Debug.Log("PlayerStatus component check refreshed");
        }
    }
    
    /// <summary>
    /// 위험도 게이지 시스템 설정
    /// </summary>
    private void SetupDangerGaugeSystem(GameObject player)
    {
        var dangerSystem = player.GetComponent<DangerGaugeSystem>();
        if (dangerSystem == null)
        {
            dangerSystem = player.AddComponent<DangerGaugeSystem>();
            Debug.Log("DangerGaugeSystem added to player");
        }
        else
        {
            Debug.Log("DangerGaugeSystem already exists on player");
        }
        
        // CharacterMove의 deadEffect를 DangerGaugeSystem에서도 사용할 수 있도록 참조 설정
        var characterMove = player.GetComponent<CharacterMove>();
        if (characterMove != null && characterMove.deadEffect != null)
        {
            Debug.Log("CharacterMove deadEffect found and will be used by DangerGaugeSystem");
        }
        
        // Flag 시스템 확인
        var flags = FindObjectsByType<Flag>(FindObjectsSortMode.None);
        if (flags.Length == 0)
        {
            Debug.LogWarning("No Flag objects found. Player will respawn at origin when dying.");
        }
        else
        {
            Debug.Log($"Found {flags.Length} Flag objects for respawn system");
        }
    }
}