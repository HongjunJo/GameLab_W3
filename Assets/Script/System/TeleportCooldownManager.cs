using UnityEngine;

/// <summary>
/// 모든 텔레포터의 쿨타임을 공유 관리하는 매니저
/// </summary>
public class TeleportCooldownManager : MonoBehaviour
{
    [Header("텔레포트 쿨타임 설정")]
    [SerializeField] private float cooldownDuration = 1.0f; // Inspector에서 설정 가능
    
    public static TeleportCooldownManager Instance { get; private set; }
    
    private float lastTeleportTime = -999f;
    
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"TeleportCooldownManager 초기화: 쿨타임 {cooldownDuration}초");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 텔레포트 가능 여부 확인
    /// </summary>
    public bool CanTeleport()
    {
        return Time.time - lastTeleportTime >= cooldownDuration;
    }
    
    /// <summary>
    /// 텔레포트 시작 - 쿨타임 즉시 적용
    /// </summary>
    public void StartTeleport()
    {
        lastTeleportTime = Time.time;
        Debug.Log($"=== 텔레포트 쿨타임 시작! {cooldownDuration}초 대기 ===");
    }
    
    /// <summary>
    /// 남은 쿨타임 시간 반환
    /// </summary>
    public float GetRemainingCooldown()
    {
        float remaining = cooldownDuration - (Time.time - lastTeleportTime);
        return Mathf.Max(0f, remaining);
    }
    
    /// <summary>
    /// 쿨타임 시간 설정 (런타임에서)
    /// </summary>
    public void SetCooldownDuration(float duration)
    {
        cooldownDuration = duration;
        Debug.Log($"텔레포트 공유 쿨타임이 {duration}초로 변경되었습니다.");
    }
    
    /// <summary>
    /// 현재 쿨타임 시간 반환
    /// </summary>
    public float GetCooldownDuration()
    {
        return cooldownDuration;
    }
    
    /// <summary>
    /// 쿨타임 즉시 리셋 (디버그용)
    /// </summary>
    public void ResetCooldown()
    {
        lastTeleportTime = -999f;
        Debug.Log("텔레포트 쿨타임 리셋!");
    }
    
    // Static 메서드들 (기존 호환성 유지)
    public static bool CanTeleportStatic()
    {
        return Instance != null && Instance.CanTeleport();
    }
    
    public static void StartTeleportStatic()
    {
        if (Instance != null) Instance.StartTeleport();
    }
    
    public static float GetRemainingCooldownStatic()
    {
        return Instance != null ? Instance.GetRemainingCooldown() : 0f;
    }
}