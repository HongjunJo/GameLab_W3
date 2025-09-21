using UnityEngine;
using System.Collections;

/// <summary>
/// 위험도 게이지 시스템 - HP 대신 위험도가 증가하여 100에 도달하면 사망
/// </summary>
public class DangerGaugeSystem : MonoBehaviour
{
    [Header("Danger Settings")]
    [SerializeField] private float maxDanger = 100f;
    [SerializeField] private float currentDanger = 0f;
    [SerializeField] private float dangerIncreaseRate = 3f; // 초당 위험도 증가량
    [SerializeField] private float dangerDecreaseRate = 10f; // 안전지대에서 초당 위험도 감소량
    
    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private ParticleSystem deathParticleEffect;
    [SerializeField] private float deathEffectDuration = 2f;
    
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 0.5f;
    [SerializeField] private bool useFlagSystem = true;
    
    [Header("Death Effects")]
    [SerializeField] private float deathFreezeTime = 1f; // 죽고 얼어있는 시간
    
    [Header("Components")]
    private CharacterMove characterMove;
    private CharacterJump characterJump;
    private Rigidbody2D playerRb;
    private PlayerStatus playerStatus;
    private SpriteRenderer spriteRenderer;
    
    [Header("State")]
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isRespawning = false;
    [SerializeField] private bool isInSafeZone = false; // isIncreasing/isDecreasing 대체
    
    // UI에서 표시할 때 100으로 클램프된 값
    public float DisplayDanger => currentDanger; // 실제 currentDanger 값 그대로 표시
    public float DangerPercentage => currentDanger / maxDanger;
    public bool IsAlive => !isDead;
    public bool IsDead => isDead;
    public bool IsInSafeZone => isInSafeZone;
    public float CurrentIncreaseRate => dangerIncreaseRate;
    public float CurrentDecreaseRate => dangerDecreaseRate;

    
    private void Awake()
    {
        characterMove = GetComponent<CharacterMove>();
        characterJump = GetComponent<CharacterJump>();
        playerRb = GetComponent<Rigidbody2D>();
        playerStatus = GetComponent<PlayerStatus>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 기본 리스폰 포인트 설정 (설정되지 않았다면 현재 위치)
        if (respawnPoint == null)
        {
            GameObject respawnObject = new GameObject("DefaultRespawnPoint");
            respawnPoint = respawnObject.transform;
            respawnPoint.position = transform.position;
        }
    }
    
    private void Start()
    {
        // 초기 위험도 이벤트 발생
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
    }
    
    private void OnEnable()
    {
        GameEvents.OnExitedSafeZone += HandleExitSafeZone;
        GameEvents.OnEnteredSafeZone += HandleEnterSafeZone;
    }
    
    private void OnDisable()
    {
        GameEvents.OnExitedSafeZone -= HandleExitSafeZone;
        GameEvents.OnEnteredSafeZone -= HandleEnterSafeZone;
    }
    
    private void Update()
    {
        if (isDead || isRespawning) return;

        bool dangerChanged = false;

        if (!isInSafeZone) // 위험 지대
        {
            // 값이 실제로 변경되었을 때만 dangerChanged를 true로 설정
            dangerChanged = IncreaseDanger(dangerIncreaseRate * Time.deltaTime);
        }
        else if (isInSafeZone && currentDanger > 0) // 안전 지대이고, 위험도가 0보다 클 때
        {
            // 값이 실제로 변경되었을 때만 dangerChanged를 true로 설정
            dangerChanged = DecreaseDanger(dangerDecreaseRate * Time.deltaTime);
        }

        if (dangerChanged) {
            GameEvents.DangerChanged(DisplayDanger, maxDanger);
        }
    }
    
    /// <summary>
    /// 위험도 증가
    /// </summary>
    public bool IncreaseDanger(float amount)
    {
        if (isDead || amount <= 0 || isRespawning) return false;
        
        currentDanger += amount;
        
        // 100 이상이 되면 사망
        if (currentDanger >= maxDanger && !isDead)
        {
            Die();
        }
        return true;
    }
    
    /// <summary>
    /// 위험도 감소
    /// </summary>
    public bool DecreaseDanger(float amount)
    {
        if (isDead || amount <= 0 || isRespawning) return false;
        
        currentDanger -= amount;
        
        // 0 미만으로 내려가지 않도록 클램프
        currentDanger = Mathf.Max(0f, currentDanger);
        
        // 0에 도달하면 완전 안전
        if (currentDanger <= 0f)
        {
            currentDanger = 0f;
            Debug.Log("Danger gauge fully recovered to 0");
        }
        return true;
    }
    
    /// <summary>
    /// 위험도 완전 초기화
    /// </summary>
    public void ResetDanger()
    {
        currentDanger = 0f;
        SetSafeZoneStatus(true); // 안전한 상태로 초기화
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
        Debug.Log("Danger gauge reset to 0");
    }
    
    private void HandleEnterSafeZone()
    {
        SetSafeZoneStatus(true);
    }

    private void HandleExitSafeZone()
    {
        SetSafeZoneStatus(false);
    }

    /// <summary>
    /// 안전지대 상태를 설정하고 관련 로직을 처리하는 중앙 메서드
    /// </summary>
    private void SetSafeZoneStatus(bool inSafeZone)
    {
        if (isDead || isRespawning) return; // 사망 또는 리스폰 중에는 상태 변경 방지

        isInSafeZone = inSafeZone;

        if (isInSafeZone) {
            Debug.Log($"안전지대 진입. 위험도 감소 시작 (현재: {currentDanger:F1})");
        } else {
            Debug.Log("위험지대 진입. 위험도 증가 시작.");
        }
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 위험도를 최대값으로 고정하여 더 이상 변화하지 않도록
        currentDanger = maxDanger;
        
        Debug.Log($"Player died from danger overload! Danger fixed at {maxDanger}");
        
        // 플레이어 사망 이벤트 발생
        GameEvents.PlayerDied();
        
        // 플레이어 제어 비활성화 및 프리징
        DisablePlayerControl();
        FreezePlayer();
        
        // 죽음 효과 시작 (메테리얼 변경, 프리징, 이펙트)
        StartCoroutine(DeathSequence());
    }
    
    /// <summary>
    /// 플레이어 제어 비활성화
    /// </summary>
    private void DisablePlayerControl()
    {
        if (characterMove != null)
        {
            characterMove.enabled = false;
        }
        
        if (characterJump != null)
        {
            characterJump.enabled = false;
        }
        
        // 물리 정지
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
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
        // 컴포넌트 재참조 (런타임 추가된 경우 대비)
        if (playerRb == null)
            playerRb = GetComponent<Rigidbody2D>();
            
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
        // 컴포넌트 재참조 (런타임 추가된 경우 대비)
        if (characterMove == null)
            characterMove = GetComponent<CharacterMove>();
        if (characterJump == null)
            characterJump = GetComponent<CharacterJump>();
        
        if (characterMove != null)
        {
            characterMove.enabled = true;
            Debug.Log("CharacterMove 활성화됨");
        }
        else
        {
            Debug.LogError("CharacterMove 컴포넌트를 찾을 수 없음!");
        }
        
        if (characterJump != null)
        {
            characterJump.enabled = true;
            Debug.Log("CharacterJump 활성화됨");
        }
        else
        {
            Debug.LogError("CharacterJump 컴포넌트를 찾을 수 없음!");
        }
        
        // InputManager 재활성화 확인
        if (InputManager.Instance != null)
        {
            InputManager.Instance.TestAble();
            Debug.Log("InputManager.TestAble() 추가 호출");
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
            RespawnToNearestFlag();
        }
        else if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            Debug.Log($"수동 리스폰 포인트로 이동: {respawnPoint.position}");
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
        currentDanger = 0f; // 위험도를 0으로 초기화

        UnfreezePlayer();
        EnablePlayerControl();
        
        if (playerStatus != null)
        {
            playerStatus.OnRespawnCompleted();
        }

        // 5. UI 업데이트: 초기화된 값(0)을 UI에 즉시 반영
        GameEvents.DangerChanged(currentDanger, maxDanger);
        Debug.Log("리스폰 완료. 위험도 0으로 초기화 및 UI 업데이트됨.");

        // 6. 한 프레임 더 대기: 물리 이벤트(OnTrigger) 등이 처리될 시간을 줌
        yield return null;

        // 7. 리스폰 상태 해제: 이제부터 정상적인 게임 로직(위험도 증가 등)이 작동 가능
        isRespawning = false;

        // 8. 새로운 위치에서 안전지대 여부 확인 (리스폰이 완전히 끝난 후)
        // isRespawning이 false가 되었으므로, 이 호출로 인해 위험도가 즉시 증가할 수 있음
        // 이 시점에는 이미 Flag의 OnTriggerEnter가 호출되어 isInSafeZone이 true일 확률이 높음
        CheckSafeZoneStatus();
    }

    /// <summary>
    /// 가장 가까운 Flag로 리스폰
    /// </summary>
    private void RespawnToNearestFlag()
    {
        Flag[] flags = FindObjectsByType<Flag>(FindObjectsSortMode.None);
        Flag nearestActiveFlag = null;
        float nearestDistance = float.MaxValue;
        
        Debug.Log($"Found {flags.Length} flags, searching for active ones...");
        
        foreach (Flag flag in flags)
        {
            Debug.Log($"Flag: {flag.name}, IsActive: {flag.IsActive}");
            if (flag.IsActive)
            {
                float distance = Vector3.Distance(transform.position, flag.transform.position);
                Debug.Log($"Active flag {flag.name} at distance {distance}");
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestActiveFlag = flag;
                }
            }
        }
        
        if (nearestActiveFlag != null)
        {
            Debug.Log($"Respawning to nearest active flag: {nearestActiveFlag.name}");
            nearestActiveFlag.TeleportPlayerHere(); // Flag의 기존 로직 사용
        }
        else if (flags.Length > 0)
        {
            Debug.Log($"No active flags found, using first flag: {flags[0].name}");
            flags[0].TeleportPlayerHere(); // 첫 번째 Flag 사용
        }
        else
        {
            Debug.LogWarning("No flags found, respawning at origin");
            transform.position = Vector3.zero;
        }
    }
    
    /// <summary>
    /// 리스폰 포인트 설정
    /// </summary>
    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint;
        Debug.Log($"Respawn point set to: {respawnPoint.position}");
    }
    
    /// <summary>
    /// 위험도 증가율 설정
    /// </summary>
    public void SetDangerIncreaseRate(float newRate)
    {
        dangerIncreaseRate = newRate;
        Debug.Log($"Danger increase rate set to: {dangerIncreaseRate}/sec");
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
        return $"Danger: {DisplayDanger:F1}/{maxDanger}, InSafeZone: {isInSafeZone}, Dead: {isDead}";
    }
    
    /// <summary>
    /// 현재 위치에서 안전지대 상태 강제 확인
    /// </summary>
    private void CheckSafeZoneStatus()
    {
        // 리스폰 중에는 이 로직을 실행하지 않음
        if (isRespawning) return;

        // 현재 위치에서 안전지대 콜라이더 확인
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        bool inSafeZone = false;

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("SafeZone")) // Flag 또는 SafeZone 오브젝트에 "SafeZone" 태그 사용 권장
            {
                inSafeZone = true;
                Debug.Log($"안전지대({collider.name}) 안에 있음.");
                break;
            }
        }

        if (inSafeZone)
        {
            SetSafeZoneStatus(true);
        }
        else
        {
            Debug.Log($"위험지대 안에 있음. 위험도 증가 로직 시작.");
            SetSafeZoneStatus(false);
        }

    }
}