using UnityEngine;

/// <summary>
/// 기존 HPDrainSystem을 비활성화하고 DangerGaugeSystem으로 교체하는 매니저
/// </summary>
public class SystemTransitionManager : MonoBehaviour
{
    [Header("System Settings")]
    [SerializeField] private bool useDangerGaugeSystem = true;
    [SerializeField] private bool disableHPSystem = true;
    [SerializeField] private bool disableHPDrainSystem = true;
    
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
        
        if (disableHPSystem)
        {
            DisableHPSystem(player);
        }
        
        if (disableHPDrainSystem)
        {
            DisableHPDrainSystem();
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
    
    /// <summary>
    /// HP 시스템 비활성화
    /// </summary>
    private void DisableHPSystem(GameObject player)
    {
        var healthComponent = player.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.enabled = false;
            Debug.Log("Health component disabled on player");
        }
        
        // HealthUI도 비활성화하거나 DangerUI로 교체
        var healthUI = FindAnyObjectByType<HealthUI>();
        if (healthUI != null)
        {
            healthUI.gameObject.SetActive(false);
            Debug.Log("HealthUI disabled");
            
            // DangerUI 추가 (같은 GameObject에)
            var dangerUI = healthUI.gameObject.AddComponent<DangerUI>();
            healthUI.gameObject.SetActive(true); // UI 오브젝트는 다시 활성화
            Debug.Log("DangerUI added to replace HealthUI");
        }
    }
    
    /// <summary>
    /// HP 드레인 시스템 비활성화
    /// </summary>
    private void DisableHPDrainSystem()
    {
        var hpDrainSystem = FindAnyObjectByType<HPDrainSystem>();
        if (hpDrainSystem != null)
        {
            hpDrainSystem.enabled = false;
            Debug.Log("HPDrainSystem disabled");
        }
    }
    
    /// <summary>
    /// 시스템 재활성화 (개발자 전용)
    /// </summary>
    [ContextMenu("Re-enable HP System")]
    public void ReEnableHPSystem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        var healthComponent = player.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.enabled = true;
        }
        
        var hpDrainSystem = FindAnyObjectByType<HPDrainSystem>();
        if (hpDrainSystem != null)
        {
            hpDrainSystem.enabled = true;
        }
        
        var healthUI = FindAnyObjectByType<HealthUI>();
        if (healthUI != null)
        {
            healthUI.gameObject.SetActive(true);
        }
        
        Debug.Log("HP System re-enabled");
    }
    
    /// <summary>
    /// 위험도 시스템 비활성화 (개발자 전용)
    /// </summary>
    [ContextMenu("Disable Danger System")]
    public void DisableDangerSystem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        var dangerSystem = player.GetComponent<DangerGaugeSystem>();
        if (dangerSystem != null)
        {
            dangerSystem.enabled = false;
        }
        
        var dangerUI = FindAnyObjectByType<DangerUI>();
        if (dangerUI != null)
        {
            dangerUI.gameObject.SetActive(false);
        }
        
        Debug.Log("Danger System disabled");
    }
}