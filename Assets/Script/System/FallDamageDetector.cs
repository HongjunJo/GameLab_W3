using UnityEngine;
using System.Collections;

/// <summary>
/// 낙사 감지 스크립트 - 이 블록에 닿으면 즉시 사망 처리
/// </summary>
public class FallDamageDetector : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("사망 처리할 플레이어 오브젝트를 직접 할당합니다. 비워두면 충돌한 'Player' 태그 오브젝트를 사용합니다.")]
    [SerializeField] private GameObject playerObject;

    [Header("Death Effect Settings")]
    [SerializeField] private GameObject deathEffect; // 사망 이펙트 프리팹
    [SerializeField] private ParticleSystem deathParticleEffect; // 파티클 이펙트
    [SerializeField] private float effectDuration = 2f; // 이펙트 지속 시간
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip fallDeathSound; // 낙사 소리
    [SerializeField] private float soundVolume = 1f; // 사운드 볼륨
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // 디버그 로그 표시 여부
    
    private bool hasTriggered = false; // 중복 트리거 방지
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 "Player" 태그를 가지고 있지 않거나, 이미 트리거되었다면 무시합니다.
        // GetComponentInParent로 찾을 것이므로, 자식 오브젝트와 충돌해도 괜찮습니다.
        if (!other.CompareTag("Player") || hasTriggered)
            return;
            
        hasTriggered = true;

        // 충돌한 'other'의 부모를 포함하여 PlayerStatus 컴포넌트를 찾습니다.
        PlayerStatus playerStatus = other.GetComponentInParent<PlayerStatus>();

        if (showDebugLogs)
        {
            Debug.Log($"=== 낙사 감지! 플레이어가 {gameObject.name}에 닿았습니다! ===");
        }

        // 이펙트와 사운드를 먼저 재생합니다.
        PlayDeathEffects(other.transform.position);
        PlayDeathSound();

        // DangerGaugeSystem을 통해 즉시 사망 및 프리징 처리를 요청합니다.
        TriggerPlayerDeath(other.gameObject); // 충돌한 오브젝트를 그대로 전달

        // 리스폰 후 다시 감지할 수 있도록 일정 시간 뒤에 트리거 상태를 리셋합니다.
        StartCoroutine(ResetTrigger());
    }
    
    /// <summary>
    /// 사망 이펙트 재생
    /// </summary>
    private void PlayDeathEffects(Vector3 playerPosition)
    {
        // 파티클 이펙트 재생
        if (deathParticleEffect != null)
        {
            // 이펙트가 플레이어를 따라다니지 않도록 월드 좌표에 독립적으로 생성
            Instantiate(deathParticleEffect, playerPosition, Quaternion.identity);
            if (showDebugLogs) Debug.Log("낙사 파티클 이펙트 생성 및 재생");
        }
        
        // 이펙트 오브젝트 생성
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, playerPosition, Quaternion.identity);
            Destroy(effect, effectDuration);
            if (showDebugLogs)
            {
                Debug.Log($"낙사 이펙트 생성: {playerPosition}");
            }
        }
    }
    
    /// <summary>
    /// 사망 사운드 재생
    /// </summary>
    private void PlayDeathSound()
    {
        if (fallDeathSound != null)
        {
            // AudioSource가 있으면 사용, 없으면 임시로 생성
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.clip = fallDeathSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
            
            if (showDebugLogs)
            {
                Debug.Log("낙사 사운드 재생");
            }
        }
    }
    
    /// <summary>
    /// 사망 처리 메서드 호출
    /// </summary>
    private void TriggerPlayerDeath(GameObject player)
    {
        // 충돌한 오브젝트의 부모를 포함하여 DangerGaugeSystem을 찾습니다.
        // 이것이 이 문제의 핵심 해결책입니다.
        DangerGaugeSystem dangerSystem = player.GetComponentInParent<DangerGaugeSystem>();

        if (dangerSystem != null)
        {
            // 이 메서드 호출 즉시 DangerGaugeSystem의 Die() -> FreezePlayer()가 실행되어
            // Rigidbody가 Kinematic으로 변경되고 그 자리에 멈추게 됩니다.
            dangerSystem.KillPlayer("Fell into a hazard"); 

        // 점프 상태를 즉시 리셋하여, 사망 직전의 점프 입력이 리스폰 후 적용되는 것을 방지합니다.
        CharacterJump characterJump = player.GetComponentInParent<CharacterJump>();
        if (characterJump != null)
        {
            characterJump.ResetJumpState();
            if (showDebugLogs) Debug.Log("CharacterJump 상태를 리셋하여 '사망 점프' 버그를 방지합니다.");
        }

            if (showDebugLogs)
            {
                Debug.Log("DangerGaugeSystem.KillPlayer() 호출 완료. 사망 및 프리징 처리를 위임합니다.");
            }
        }
        else
        {
            // DangerGaugeSystem이 없는 경우를 대비한 경고 로그
            Debug.LogError("플레이어에서 DangerGaugeSystem을 찾을 수 없어 프리징 처리를 할 수 없습니다! 플레이어가 계속 떨어질 수 있습니다.");
            // 백업으로 GameEvents를 직접 호출할 수 있지만, 프리징은 보장되지 않습니다.
            GameEvents.PlayerDied();
        }
    }

    /// <summary>
    /// 일정 시간 후 트리거를 다시 활성화하는 코루틴
    /// </summary>
    private IEnumerator ResetTrigger()
    {
        // 플레이어가 리스폰할 시간을 충분히 줍니다.
        yield return new WaitForSeconds(3f);
        hasTriggered = false;
    }
    
    /// <summary>
    /// 디버그용 기즈모 표시
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}