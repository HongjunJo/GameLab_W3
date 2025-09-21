using UnityEngine;
using System.Collections;

/// <summary>
/// 낙사 감지 스크립트 - 이 블록에 닿으면 즉시 사망 처리
/// </summary>
public class FallDamageDetector : MonoBehaviour
{
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
        // 플레이어만 감지
        if (!other.CompareTag("Player") || hasTriggered)
            return;
            
        hasTriggered = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"=== 낙사 감지! 플레이어가 {gameObject.name}에 닿았습니다! ===");
        }
        
        // 즉시 플레이어 제어 차단 및 사망 처리
        StartCoroutine(HandleFallDeath(other.gameObject));
    }
    
    /// <summary>
    /// 낙사 처리 시퀀스
    /// </summary>
    private IEnumerator HandleFallDeath(GameObject player)
    {
        // 1. 즉시 입력 및 이동 차단
        DisablePlayerInput(player);
        DisablePlayerMovement(player);
        
        if (showDebugLogs)
        {
            Debug.Log("플레이어 입력 및 이동 차단 완료");
        }
        
        // 2. 이펙트 재생
        PlayDeathEffects(player.transform.position);
        
        // 3. 사운드 재생
        PlayDeathSound();
        
        // 4. 짧은 대기 (이펙트가 보이도록)
        yield return new WaitForSeconds(0.1f);
        
        // 5. 사망 처리 메서드 호출
        TriggerPlayerDeath(player);
        
        if (showDebugLogs)
        {
            Debug.Log("낙사 처리 완료 - 사망 시스템으로 이관");
        }
        
        // 6. 잠시 후 트리거 리셋 (다른 플레이어나 재시도를 위해)
        yield return new WaitForSeconds(2f);
        hasTriggered = false;
    }
    
    /// <summary>
    /// 플레이어 입력 시스템 비활성화
    /// </summary>
    private void DisablePlayerInput(GameObject player)
    {
        // InputManager의 TestDisable 사용 (텔레포터와 동일한 방식)
        if (InputManager.Instance != null)
        {
            InputManager.Instance.TestDisable();
            if (showDebugLogs)
            {
                Debug.Log("InputManager.TestDisable() 호출 완료");
            }
        }
        else
        {
            // 백업 방법: 기본 Input 리셋
            Input.ResetInputAxes();
            if (showDebugLogs)
            {
                Debug.LogWarning("InputManager가 없어 Input.ResetInputAxes() 사용");
            }
        }
    }
    
    /// <summary>
    /// 플레이어 이동 및 물리 정지
    /// </summary>
    private void DisablePlayerMovement(GameObject player)
    {
        // CharacterMove 비활성화
        CharacterMove characterMove = player.GetComponent<CharacterMove>();
        if (characterMove != null)
        {
            characterMove.directionX = 0f;
            characterMove.pressingKey = false;
            characterMove.velocity = Vector2.zero;
            characterMove.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log("CharacterMove 비활성화 완료");
            }
        }
        
        // CharacterJump 비활성화
        CharacterJump characterJump = player.GetComponent<CharacterJump>();
        if (characterJump != null)
        {
            characterJump.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log("CharacterJump 비활성화 완료");
            }
        }
        
        // Rigidbody2D 완전 정지
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.bodyType = RigidbodyType2D.Kinematic; // 물리 영향 차단
            if (showDebugLogs)
            {
                Debug.Log("Rigidbody2D 프리징 완료");
            }
        }
        
        // InputSystemPlayerController 비활성화 (있다면)
        var inputController = player.GetComponent<InputSystemPlayerController>();
        if (inputController != null)
        {
            inputController.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log("InputSystemPlayerController 비활성화 완료");
            }
        }
    }
    
    /// <summary>
    /// 사망 이펙트 재생
    /// </summary>
    private void PlayDeathEffects(Vector3 position)
    {
        // 파티클 이펙트 재생
        if (deathParticleEffect != null)
        {
            if (!deathParticleEffect.isPlaying)
            {
                deathParticleEffect.transform.position = position;
                deathParticleEffect.Play();
                if (showDebugLogs)
                {
                    Debug.Log("낙사 파티클 이펙트 재생");
                }
            }
        }
        
        // 이펙트 오브젝트 생성
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, position, Quaternion.identity);
            Destroy(effect, effectDuration);
            if (showDebugLogs)
            {
                Debug.Log($"낙사 이펙트 생성: {position}");
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
        if (showDebugLogs)
        {
            Debug.Log($"=== TriggerPlayerDeath 호출됨. 플레이어 오브젝트: {player.name} ===");
        }
        
        // 1. DangerGaugeSystem 우선 처리 - 위험도를 최대치로 올려서 즉시 사망 처리
        DangerGaugeSystem dangerSystem = player.GetComponent<DangerGaugeSystem>();
        if (showDebugLogs)
        {
            Debug.Log($"DangerGaugeSystem 컴포넌트 검색 결과: {(dangerSystem != null ? "발견됨" : "null")}");
            
            // 모든 컴포넌트 나열
            var allComponents = player.GetComponents<MonoBehaviour>();
            Debug.Log($"플레이어의 모든 MonoBehaviour 컴포넌트 ({allComponents.Length}개):");
            foreach (var comp in allComponents)
            {
                Debug.Log($"  - {comp.GetType().Name}");
            }
        }
        
        if (dangerSystem != null)
        {
            // 위험도를 최대치로 설정하여 자동으로 사망 처리되도록 함
            dangerSystem.IncreaseDanger(999999f);
            if (showDebugLogs)
            {
                Debug.Log("DangerGaugeSystem 위험도 최대치로 설정 - 자동 사망 처리");
            }
            return;
        }
        
        // 2. Health 시스템이 있다면 즉시 사망 처리
        Health healthComponent = player.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(999999f); // 충분히 큰 데미지로 즉시 사망
            if (showDebugLogs)
            {
                Debug.Log("Health 시스템으로 즉시 사망 처리");
            }
            return;
        }
        
        // 3. 백업: 태그로 Player 찾아서 직접 DangerGaugeSystem 호출 시도
        GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
        if (playerByTag != null)
        {
            DangerGaugeSystem dangerSystemByTag = playerByTag.GetComponent<DangerGaugeSystem>();
            if (dangerSystemByTag != null)
            {
                dangerSystemByTag.IncreaseDanger(999999f);
                if (showDebugLogs)
                {
                    Debug.Log("태그로 찾은 Player의 DangerGaugeSystem으로 사망 처리");
                }
                return;
            }
            else
            {
                // Player에 DangerGaugeSystem이 없다면 즉시 추가
                DangerGaugeSystem newDangerSystem = playerByTag.AddComponent<DangerGaugeSystem>();
                newDangerSystem.IncreaseDanger(999999f);
                if (showDebugLogs)
                {
                    Debug.Log("Player에 DangerGaugeSystem 추가 후 사망 처리");
                }
                return;
            }
        }
        
        // 4. Scene에서 DangerGaugeSystem 직접 검색 시도
        DangerGaugeSystem anyDangerSystem = FindAnyObjectByType<DangerGaugeSystem>();
        if (anyDangerSystem != null)
        {
            anyDangerSystem.IncreaseDanger(999999f);
            if (showDebugLogs)
            {
                Debug.Log("Scene에서 찾은 DangerGaugeSystem으로 사망 처리");
            }
            return;
        }
        
        // 5. 최종 백업: GameEvents로 사망 이벤트 발생
        GameEvents.PlayerDied();
        if (showDebugLogs)
        {
            Debug.Log("GameEvents.PlayerDied() 직접 호출");
        }
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