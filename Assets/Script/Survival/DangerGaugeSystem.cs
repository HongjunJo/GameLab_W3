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
    [SerializeField] private float dangerIncreaseRate = 5f; // 초당 위험도 증가량
    [SerializeField] private float dangerDecreaseRate = 2f; // 안전지대에서 초당 위험도 감소량
    [SerializeField] private bool isIncreasing = false;
    [SerializeField] private bool isDecreasing = false;
    
    [Header("Death Settings")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private ParticleSystem deathParticleEffect;
    [SerializeField] private float deathEffectDuration = 2f;
    
    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private bool useFlagSystem = true;
    
    [Header("Death Effects")]
    [SerializeField] private Material deathMaterial; // 죽을 때 적용할 메테리얼
    [SerializeField] private float deathFreezeTime = 1f; // 죽고 얼어있는 시간
    [SerializeField] private float materialChangeSpeed = 0.5f; // 메테리얼 변경 속도
    
    [Header("Components")]
    private CharacterMove characterMove;
    private CharacterJump characterJump;
    private Rigidbody2D playerRb;
    private PlayerStatus playerStatus;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial; // 원본 메테리얼 저장
    
    [Header("State")]
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isRespawning = false;
    
    // UI에서 표시할 때 100으로 클램프된 값
    public float DisplayDanger => Mathf.Min(currentDanger, maxDanger);
    public float DangerPercentage => currentDanger / maxDanger;
    public bool IsAlive => !isDead;
    public bool IsDead => isDead;
    
    private void Awake()
    {
        characterMove = GetComponent<CharacterMove>();
        characterJump = GetComponent<CharacterJump>();
        playerRb = GetComponent<Rigidbody2D>();
        playerStatus = GetComponent<PlayerStatus>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 원본 메테리얼 저장
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
        
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
        GameEvents.OnExitedSafeZone += StartDangerIncrease;
        GameEvents.OnEnteredSafeZone += StopDangerIncrease;
    }
    
    private void OnDisable()
    {
        GameEvents.OnExitedSafeZone -= StartDangerIncrease;
        GameEvents.OnEnteredSafeZone -= StopDangerIncrease;
    }
    
    private void Update()
    {
        if (isDead || isRespawning) return;
        
        if (isIncreasing)
        {
            float previousDanger = currentDanger;
            IncreaseDanger(dangerIncreaseRate * Time.deltaTime);
            
            // 10씩 증가할 때마다 로그 출력 (너무 많은 로그 방지)
            if (Mathf.FloorToInt(currentDanger / 10f) > Mathf.FloorToInt(previousDanger / 10f))
            {
                Debug.Log($"Danger gauge: {currentDanger:F1}/{maxDanger}");
            }
        }
        else if (isDecreasing && currentDanger > 0)
        {
            float previousDanger = currentDanger;
            DecreaseDanger(dangerDecreaseRate * Time.deltaTime);
            
            // 10씩 감소할 때마다 로그 출력
            if (Mathf.FloorToInt(currentDanger / 10f) < Mathf.FloorToInt(previousDanger / 10f))
            {
                Debug.Log($"Danger gauge decreasing: {currentDanger:F1}/{maxDanger}");
            }
        }
    }
    
    /// <summary>
    /// 위험도 증가
    /// </summary>
    public void IncreaseDanger(float amount)
    {
        if (isDead || amount <= 0) return;
        
        currentDanger += amount;
        
        // UI 업데이트 (100으로 클램프된 값 표시)
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
        
        // 100 이상이 되면 사망
        if (currentDanger >= maxDanger)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 위험도 감소
    /// </summary>
    public void DecreaseDanger(float amount)
    {
        if (isDead || amount <= 0) return;
        
        currentDanger -= amount;
        
        // 0 미만으로 내려가지 않도록 클램프
        currentDanger = Mathf.Max(0f, currentDanger);
        
        // UI 업데이트
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
        
        // 0에 도달하면 완전 안전
        if (currentDanger <= 0f)
        {
            currentDanger = 0f;
            isDecreasing = false; // 감소 중단
            Debug.Log("Danger gauge fully recovered to 0");
        }
    }
    
    /// <summary>
    /// 위험도 완전 초기화
    /// </summary>
    public void ResetDanger()
    {
        currentDanger = 0f;
        isIncreasing = false;
        isDecreasing = false; // 감소 상태도 초기화
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
        Debug.Log("Danger gauge reset to 0");
    }
    
    /// <summary>
    /// 리스폰 후 시스템 상태 완전 초기화
    /// </summary>
    private void ResetSystemState()
    {
        // 위험도 초기화
        currentDanger = 0f;
        isIncreasing = false;
        isDecreasing = false; // 감소 상태도 초기화
        isDead = false;
        isRespawning = false;
        
        // 메테리얼 복구 (안전장치)
        RestoreOriginalMaterial();
        
        // 이벤트 재구독 (혹시 끊어졌을 수도 있으므로)
        GameEvents.OnExitedSafeZone -= StartDangerIncrease;
        GameEvents.OnEnteredSafeZone -= StopDangerIncrease;
        GameEvents.OnExitedSafeZone += StartDangerIncrease;
        GameEvents.OnEnteredSafeZone += StopDangerIncrease;
        
        // UI 업데이트
        GameEvents.DangerChanged(DisplayDanger, maxDanger);
        
        Debug.Log("System state completely reset after respawn");
    }
    
    /// <summary>
    /// 위험도 증가 시작 (안전지대를 벗어났을 때)
    /// </summary>
    private void StartDangerIncrease()
    {
        if (!isDead)
        {
            isIncreasing = true;
            isDecreasing = false; // 감소 중단
            Debug.Log("Danger gauge started increasing");
        }
    }
    
    /// <summary>
    /// 위험도 증가 중단 (안전지대에 들어왔을 때)
    /// </summary>
    private void StopDangerIncrease()
    {
        isIncreasing = false;
        
        // 안전지대에 들어왔고 위험도가 0보다 크면 감소 시작
        if (!isDead && currentDanger > 0)
        {
            isDecreasing = true;
            Debug.Log("Danger gauge started decreasing in safe zone");
        }
        else
        {
            Debug.Log("Danger gauge stopped increasing");
        }
    }
    
    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        isIncreasing = false;
        
        Debug.Log("Player died from danger overload!");
        
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
        if (playerRb != null)
        {
            // 다이나믹으로 복원
            playerRb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        Debug.Log("Player unfrozen");
    }
    
    /// <summary>
    /// 죽음 시퀀스 (메테리얼 변경 → 프리징 → 이펙트 → 리스폰)
    /// </summary>
    private IEnumerator DeathSequence()
    {
        Debug.Log("Starting death sequence...");
        
        // 1. 메테리얼 변경 (즉시 또는 페이드)
        if (deathMaterial != null && spriteRenderer != null)
        {
            yield return StartCoroutine(ChangeMaterialCoroutine(deathMaterial));
        }
        
        // 2. 죽음 이펙트 재생
        PlayDeathEffect();
        
        // 3. 프리징 시간 대기
        yield return new WaitForSeconds(deathFreezeTime);
        
        // 4. 리스폰 시작
        StartCoroutine(RespawnCoroutine());
    }
    
    /// <summary>
    /// 메테리얼 변경 코루틴
    /// </summary>
    private IEnumerator ChangeMaterialCoroutine(Material targetMaterial)
    {
        if (spriteRenderer == null) yield break;
        
        Debug.Log($"Changing material to: {targetMaterial.name}");
        
        // 즉시 변경 (페이드 효과를 원한다면 여기서 구현 가능)
        spriteRenderer.material = targetMaterial;
        
        yield return new WaitForSeconds(materialChangeSpeed);
        
        Debug.Log("Material change completed");
    }
    
    /// <summary>
    /// 메테리얼 원상복구
    /// </summary>
    private void RestoreOriginalMaterial()
    {
        if (spriteRenderer != null && originalMaterial != null)
        {
            spriteRenderer.material = originalMaterial;
            Debug.Log("Original material restored");
        }
    }
    
    /// <summary>
    /// 플레이어 제어 활성화
    /// </summary>
    private void EnablePlayerControl()
    {
        if (characterMove != null)
        {
            characterMove.enabled = true;
        }
        
        if (characterJump != null)
        {
            characterJump.enabled = true;
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
    /// 리스폰 코루틴
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        
        // 리스폰 대기
        yield return new WaitForSeconds(respawnDelay);
        
        // 리스폰 처리
        if (useFlagSystem)
        {
            RespawnToNearestFlag();
        }
        else if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            Debug.Log($"Respawned to manual respawn point: {respawnPoint.position}");
        }
        else
        {
            transform.position = Vector3.zero;
            Debug.Log("Respawned to origin (no respawn point set)");
        }
        
        // 메테리얼 원상복구
        RestoreOriginalMaterial();
        
        // 프리징 해제
        UnfreezePlayer();
        
        // 시스템 상태 완전 초기화
        ResetSystemState();
        
        // 플레이어 제어 다시 활성화
        EnablePlayerControl();
        
        // PlayerStatus에 리스폰 완료 알림
        if (playerStatus != null)
        {
            playerStatus.OnRespawnCompleted();
        }
        
        // 리스폰 후 안전지대 상태 강제 확인
        yield return new WaitForSeconds(0.1f); // 위치 이동 후 잠시 대기
        CheckSafeZoneStatus();
        
        Debug.Log("Player respawn sequence completed!");
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
        return $"Danger: {DisplayDanger:F1}/{maxDanger}, Increasing: {isIncreasing}, Dead: {isDead}";
    }
    
    /// <summary>
    /// 현재 위치에서 안전지대 상태 강제 확인
    /// </summary>
    private void CheckSafeZoneStatus()
    {
        // 현재 위치에서 안전지대 확인
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        bool inSafeZone = false;
        
        foreach (var collider in colliders)
        {
            SafeZone safeZone = collider.GetComponent<SafeZone>();
            if (safeZone != null && safeZone.IsActive)
            {
                inSafeZone = true;
                Debug.Log($"Player is in safe zone: {safeZone.name}");
                break;
            }
        }
        
        if (inSafeZone)
        {
            StopDangerIncrease();
        }
        else
        {
            StartDangerIncrease();
        }
        
        // PlayerStatus 업데이트
        if (playerStatus != null)
        {
            playerStatus.SetSafeZoneStatus(inSafeZone);
        }
    }
}