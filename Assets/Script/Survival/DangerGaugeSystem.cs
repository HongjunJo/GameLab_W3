using UnityEngine;
using System.Collections;

/// <summary>
/// 산소 게이지 시스템 - 산소가 서서히 감소하여 0에 도달하면 사망
/// </summary>
public class DangerGaugeSystem : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float currentOxygen = 100f; // 100에서 시작
    [SerializeField] private float oxygenDecreaseRate = 3f; // 초당 산소 소모량
    [SerializeField] private float oxygenIncreaseRate = 10f; // 안전지대에서 초당 산소 회복량
    
    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private ParticleSystem deathParticleEffect;
    [SerializeField] private float deathEffectDuration = 2f;
    
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private bool useFlagSystem = true;
    
    [Header("Death Effects")]
    [SerializeField] private float deathFreezeTime = 1f; // 죽고 얼어있는 시간
    
    [Header("Components")]
    private CharacterMove characterMove;
    private CharacterJump characterJump;
    [SerializeField] private Rigidbody2D playerRb; // 인스펙터에서 할당할 수 있도록 변경
    private PlayerStatus playerStatus;
    private SpriteRenderer spriteRenderer;
    
    [Header("State")]
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isRespawning = false;
    [SerializeField] private bool isInSafeZone = false; // isIncreasing/isDecreasing 대체
    
    // UI에서 표시할 때 100으로 클램프된 값
    public float DisplayDanger => currentOxygen; // 실제 currentOxygen 값 그대로 표시
    public float DangerPercentage => currentOxygen / maxOxygen;
    public bool IsAlive => !isDead;
    public bool IsDead => isDead;
    public bool IsInSafeZone => isInSafeZone;
    public float CurrentIncreaseRate => oxygenIncreaseRate; // 이름은 유지하되, 회복률을 반환
    public float CurrentDecreaseRate => oxygenDecreaseRate; // 이름은 유지하되, 소모율을 반환

    
    private void Awake()
    {
        characterMove = GetComponent<CharacterMove>();
        characterJump = GetComponent<CharacterJump>();
        // playerRb = GetComponentInChildren<Rigidbody2D>(); // 더 이상 자식에서 찾지 않음
        playerStatus = GetComponent<PlayerStatus>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (playerRb == null)
        {
            Debug.LogError("Player의 Rigidbody2D가 DangerGaugeSystem의 인스펙터에 할당되지 않았습니다!");
        }
    }
    
    private void Start()
    {
        // 초기 위험도 이벤트 발생
        currentOxygen = maxOxygen; // 시작 시 산소 가득 채움
        GameEvents.DangerChanged(DisplayDanger, maxOxygen);
    }
    
    private void Update()
    {
        if (isDead || isRespawning) return;

        // 매 프레임 안전지대 상태를 직접 확인합니다.
        // 이 방식이 OnTriggerStay와 동일한 안정성을 제공합니다.
        bool currentlyInSafeZone = IsPlayerInSafeZone();
        if (isInSafeZone != currentlyInSafeZone) {
            SetSafeZoneStatus(currentlyInSafeZone);
        }

        bool dangerChanged = false;

        if (!isInSafeZone) // 위험 지대
        {
            // 산소 소모
            dangerChanged = DecreaseDanger(oxygenDecreaseRate * Time.deltaTime);
        }
        else if (isInSafeZone && currentOxygen < maxOxygen) // 안전 지대이고, 산소가 가득 차지 않았을 때
        {
            // 산소 회복
            dangerChanged = IncreaseDanger(oxygenIncreaseRate * Time.deltaTime);
        }
        
        if (dangerChanged) {
            GameEvents.DangerChanged(DisplayDanger, maxOxygen);
        }
    }
    
    /// <summary>
    /// 위험도 증가
    /// (산소 시스템에서는 '회복'의 의미로 사용)
    /// </summary>
    public bool IncreaseDanger(float amount)
    {
        if (isDead || amount <= 0 || isRespawning) return false;
        
        currentOxygen += amount;
        currentOxygen = Mathf.Min(currentOxygen, maxOxygen); // 최대치를 넘지 않도록
        
        return true;
    }
    
    /// <summary>
    /// 위험도 감소
    /// (산소 시스템에서는 '소모'의 의미로 사용)
    /// </summary>
    public bool DecreaseDanger(float amount)
    {
        if (isDead || amount <= 0 || isRespawning) return false;
        
        currentOxygen -= amount;
        
        // 0 이하가 되면 사망
        if (currentOxygen <= 0f && !isDead)
        {
            currentOxygen = 0f;
            Die();
        }
        return true;
    }
    
    /// <summary>
    /// 위험도 완전 초기화
    /// (산소 시스템에서는 '완전 회복'의 의미로 사용)
    /// </summary>
    public void ResetDanger()
    {
        currentOxygen = maxOxygen;
        SetSafeZoneStatus(true); // 안전한 상태로 초기화
        GameEvents.DangerChanged(DisplayDanger, maxOxygen);
        Debug.Log("Oxygen gauge reset to 100");
    }
    
    /// <summary>
    /// 안전지대 상태를 설정하고 관련 로직을 처리하는 중앙 메서드
    /// </summary>
    private void SetSafeZoneStatus(bool inSafeZone)
    {
        if (isDead) return; // 사망 중에는 상태 변경 방지 (리스폰 중에는 허용)

        // 상태가 실제로 변경될 때만 로그를 출력하여 중복을 피합니다.
        if (isInSafeZone == inSafeZone) return;

        isInSafeZone = inSafeZone;

        if (isInSafeZone) {
            Debug.Log($"안전지대 진입. 산소 회복 시작 (현재: {currentOxygen:F1})");
        } else {
            Debug.Log($"위험지대 진입. 산소 소모 시작. (현재: {currentOxygen:F1})");
        }
    }
    
    /// <summary>
    /// 외부 시스템에서 플레이어를 즉시 사망 처리할 때 호출
    /// </summary>
    /// <param name="cause">사망 원인 (로그용)</param>
    public void KillPlayer(string cause)
    {
        Debug.Log($"플레이어 즉시 사망 처리 요청. 원인: {cause}");
        currentOxygen = 0f;
        Die();
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 위험도를 최대값으로 고정하여 더 이상 변화하지 않도록
        currentOxygen = 0f;
        
        Debug.Log($"Player died from oxygen depletion! Oxygen fixed at 0");
        
        // 플레이어 사망 이벤트 발생
        GameEvents.PlayerDied();
        
        // 사망 직전의 점프 입력을 포함한 모든 점프 상태를 즉시 리셋합니다.
        if (characterJump != null)
        {
            characterJump.ResetJumpState();
            Debug.Log("사망 처리 시작: CharacterJump 상태를 리셋하여 원치 않는 점프를 방지합니다.");
        }

        // 플레이어 제어 비활성화 및 프리징
        DisablePlayerControl();
        FreezePlayer();

        // MovementLimiter를 통해 추가적으로 움직임 제한
        if (MovementLimiter.Instance != null)
        {
            MovementLimiter.Instance.SetCanMove(false);
        }
        
        // 죽음 효과 시작 (메테리얼 변경, 프리징, 이펙트)
        StartCoroutine(DeathSequence());
    }
    
    /// <summary>
    /// 플레이어 제어 비활성화
    /// </summary>
    private void DisablePlayerControl()
    {
        // .enabled를 직접 제어하는 대신 MovementLimiter를 사용합니다.
        // characterMove.enabled = false;
        // characterJump.enabled = false;
        Debug.Log("Player control disabled via MovementLimiter.");

        // 물리 정지
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }
    
    /// <summary>
    /// 플레이어 프리징 (물리적 고정)
    /// </summary>
    private void FreezePlayer()
    {
        if (playerRb != null)
        {
            // 물리 완전 정지
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            // 일시적으로 키네마틱으로 변경하여 물리 영향 차단
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        Debug.Log("Player frozen");
    }
    
    /// <summary>
    /// 플레이어 프리징 해제
    /// </summary>
    private void UnfreezePlayer()
    {
        // playerRb 참조가 유실되었을 경우를 대비해 다시 가져옵니다.
        if (playerRb == null)
        {
            Debug.LogError("Player의 Rigidbody2D 참조가 유실되었습니다. 인스펙터 할당을 확인하세요.");
        }

        if (playerRb != null)
        {
            // 다이나믹으로 복원
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            // 물리 상태 완전 초기화
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            // 물리 제약 해제
            playerRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("Rigidbody2D 완전 초기화 및 프리징 해제");
        }
        else
        {
            Debug.LogError("Rigidbody2D 컴포넌트를 찾을 수 없음!");
        }
        
        Debug.Log("Player unfrozen");
    }
    
    /// <summary>
    /// 죽음 시퀀스 (이펙트와 프리징 동시 실행 → 0.5초 대기 → 리스폰)
    /// </summary>
    private IEnumerator DeathSequence()
    {
        Debug.Log("Starting death sequence...");
        
        // 1. 죽음 이펙트 재생과 프리징을 동시에 시작
        PlayDeathEffect();
        
        // 2. 프리징 시간 대기 (이펙트와 동시 실행)
        yield return new WaitForSeconds(deathFreezeTime); // 1초
        
        // 3. 추가 0.5초 대기 후 리스폰 시작
        yield return new WaitForSeconds(0.5f);
        
        // 4. 리스폰 시작
        StartCoroutine(RespawnCoroutine());
    }
    
    /// <summary>
    /// 플레이어 제어 활성화
    /// </summary>
    private void EnablePlayerControl()
    {
        // .enabled를 직접 제어하는 대신 MovementLimiter를 사용합니다.
        // characterMove.enabled = true;
        // characterJump.enabled = true;

        // MovementLimiter를 통해 움직임 제한 해제
        if (MovementLimiter.Instance != null)
        {
            MovementLimiter.Instance.SetCanMove(true);
            Debug.Log("MovementLimiter를 통해 플레이어 제어 복구됨");
        }
    }
    
    /// <summary>
    /// 사망 이펙트 재생
    /// </summary>
    private void PlayDeathEffect()
    {
        Debug.Log("Playing death effects...");
        
        // 파티클 이펙트 재생
        if (deathParticleEffect != null)
        {
            if (!deathParticleEffect.isPlaying)
            {
                deathParticleEffect.Play();
                Debug.Log("Death particle effect played");
            }
        }
        else
        {
            Debug.LogWarning("Death particle effect is null");
        }
        
        // CharacterMove의 deadEffect와 충돌하지 않도록 별도 이펙트 사용
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            Debug.Log($"Death effect instantiated at {transform.position}");
            
            // 자동 제거
            Destroy(effect, deathEffectDuration);
        }
        else
        {
            Debug.LogWarning("Death effect prefab is null");
        }
        
        // CharacterMove의 deadEffect도 재생 (있다면)
        var characterMove = GetComponent<CharacterMove>();
        if (characterMove != null && characterMove.deadEffect != null)
        {
            if (!characterMove.deadEffect.isPlaying)
            {
                characterMove.deadEffect.Play();
                Debug.Log("CharacterMove death effect played");
            }
        }
    }
    
    /// <summary>
    /// 리스폰 코루틴 (수정된 버전)
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        Debug.Log("리스폰 시퀀스 시작.");

        // 1. 리스폰 딜레이
        yield return new WaitForSeconds(respawnDelay);

        // 2. 리스폰 위치로 플레이어 이동
        if (useFlagSystem)
        {
            // PlayerStatus에서 현재 섹터 정보를 가져옵니다.
            if (playerStatus != null && playerStatus.CurrentSector != null)
            {
                // 섹터에 지정된 리스폰 포인트가 있다면 그곳으로 이동합니다.
                Transform sectorRespawnPoint = playerStatus.CurrentSector.RespawnPoint;
                Vector3 targetPos = new Vector3(sectorRespawnPoint.position.x, sectorRespawnPoint.position.y, 0);
                
                Debug.Log($"'{playerStatus.CurrentSector.SectorName}' 섹터의 지정된 위치({sectorRespawnPoint.name})에서 리스폰합니다. -> {targetPos}");

                if (playerRb != null)
                {
                    playerRb.transform.position = targetPos;
                }
                else
                {
                    Debug.LogError("리스폰할 대상(playerRb)이 지정되지 않았습니다!");
                }
            }
            else
            {
                // 현재 속한 섹터가 없다면, 메인 Flag에서 리스폰합니다.
                Debug.Log("현재 속한 섹터가 없어, 메인 Flag를 찾습니다.");
                if (playerRb != null) RespawnToMainFlag(playerRb.transform);
            }
        }
        else
        {
            transform.position = Vector3.zero;
            Debug.Log("리스폰 포인트 없음, 원점으로 이동");
        }

        // 3. 순간이동 후 물리 엔진이 새 위치를 인식하도록 한 프레임 대기
        yield return new WaitForFixedUpdate();

        // 4. 플레이어 상태 초기화 (가장 중요)
        isDead = false;
        currentOxygen = maxOxygen; // 산소를 100으로 초기화

        UnfreezePlayer();
        EnablePlayerControl();
        
        if (playerStatus != null)
        {
            playerStatus.OnRespawnCompleted();
        }

        // 5. UI 업데이트: 초기화된 값(0)을 UI에 즉시 반영
        GameEvents.DangerChanged(currentOxygen, maxOxygen);
        Debug.Log("리스폰 완료. 산소 100으로 초기화 및 UI 업데이트됨.");
        
        // 리스폰 후 최종 위치 확인
        if (playerRb != null)
        {
            Debug.Log($"[최종 위치 확인] 리스폰 후 플레이어 위치: {playerRb.transform.position}");
        }

        // 6. 한 프레임 더 대기: 물리 이벤트(OnTrigger) 등이 처리될 시간을 줌
        yield return null;

        // 7. 리스폰 상태를 먼저 해제
        isRespawning = false;

        // 8. 새로운 위치에서 안전지대 여부 즉시 확인
        SetSafeZoneStatus(IsPlayerInSafeZone());
    }

    /// <summary>
    /// 메인 Flag로 리스폰합니다.
    /// </summary>
    private void RespawnToMainFlag(Transform playerTransform)
    {
        Flag[] flags = FindObjectsByType<Flag>(FindObjectsSortMode.None);
        Flag mainFlag = null;
        
        Debug.Log($"Found {flags.Length} flags, searching for the main flag...");
        
        foreach (Flag flag in flags)
        {
            if (flag.isMainFlag)
            {
                mainFlag = flag;
                break;
            }
        }
        
        if (mainFlag != null)
        {
            Debug.Log($"메인 Flag에서 리스폰합니다: {mainFlag.name}");
            Vector3 targetPos = new Vector3(mainFlag.transform.position.x, mainFlag.transform.position.y, 0);
            playerTransform.position = targetPos;
            Debug.Log($"플레이어 위치를 {targetPos}로 이동시킴");
        }
        else
        {
            Debug.LogWarning("메인 Flag를 찾을 수 없어 원점(0,0,0)으로 리스폰합니다.");
            playerTransform.position = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 위험도 증가율 설정
    /// </summary>
    public void SetOxygenDecreaseRate(float newRate)
    {
        oxygenDecreaseRate = newRate;
        Debug.Log($"Oxygen decrease rate set to: {oxygenDecreaseRate}/sec");
    }
    
    /// <summary>
    /// 강제 리스폰
    /// </summary>
    public void ForceRespawn()
    {
        if (!isDead)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 현재 위험도 정보
    /// </summary>
    public string GetDangerInfo()
    {
        return $"Oxygen: {DisplayDanger:F1}/{maxOxygen}, InSafeZone: {isInSafeZone}, Dead: {isDead}";
    }
    
    /// <summary>
    /// 현재 위치에서 안전지대 상태 강제 확인
    /// </summary>
    public bool IsPlayerInSafeZone()
    {
        // 현재 위치에서 안전지대 콜라이더 확인
        if (playerRb == null)
        {
            Debug.LogError("CheckSafeZoneStatus: playerRb가 할당되지 않아 안전지대 상태를 확인할 수 없습니다.");
            return false;
        }

        // 플레이어의 위치를 기준으로 안전지대 콜라이더를 확인합니다.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerRb.transform.position, 0.1f);

        foreach (var collider in colliders)
        {
            // SafeZone 컴포넌트가 있고, 해당 SafeZone이 활성화 상태인지 확인합니다.
            SafeZone zone = collider.GetComponent<SafeZone>();
            if (zone != null && zone.IsActive)
            {
                return true;
            }
        }
        return false;
    }
}