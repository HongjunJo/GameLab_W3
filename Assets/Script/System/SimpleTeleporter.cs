using UnityEngine;
using System.Collections;

/// <summary>
/// 간단한 텔레포터 - W키로 특정 위치로 이동
/// </summary>
public class SimpleTeleporter : MonoBehaviour
{
    [Header("텔레포트 설정")]
    [SerializeField] private Transform teleportTarget; // 이동할 위치
    [SerializeField] private KeyCode teleportKey = KeyCode.W; // 텔레포트 키
    [SerializeField] private float interactionRange = 3f; // 상호작용 범위
    [SerializeField] private Vector2 positionOffset = new Vector2(0f, -2f); // 위치 오프셋 (X, Y)
    
    [Header("페이드 효과")]
    [SerializeField] private float fadeTime = 0.5f; // 페이드 시간
    [SerializeField] private Color fadeColor = Color.black; // 페이드 색상
    
    [Header("비용 (선택사항)")]
    [SerializeField] private MineralData requiredMineral; // 필요한 광물
    [SerializeField] private int requiredAmount = 0; // 필요한 수량
    
    [Header("조건 확인 (선택사항)")]
    [SerializeField] private Mine requiredMine; // 활성화되어야 하는 광산
    [SerializeField] private bool checkMineActive = true; // 광산 활성화 상태 확인 여부
    
    private GameObject player;
    private bool isTeleporting = false;
    private bool playerInRange = false;
    
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }
    
    private void Update()
    {
        // 텔레포트 중이거나 공유 쿨타임 중이면 모든 입력 무시
        if (isTeleporting || (TeleportCooldownManager.Instance != null && !TeleportCooldownManager.Instance.CanTeleport())) 
        {
            return;
        }
        
        // 플레이어가 범위 안에 있고 W키를 눌렀을 때
        if (playerInRange && Input.GetKeyDown(teleportKey))
        {
            // 즉시 모든 입력 차단
            Input.ResetInputAxes();
            Debug.Log("=== W키 입력 감지! 모든 입력 즉시 차단 ===");
            
            if (CanTeleport())
            {
                StartCoroutine(TeleportSequence());
            }
            else
            {
                Debug.Log("텔레포트 할 수 없습니다!");
            }
        }
    }
    
    /// <summary>
    /// 텔레포트 가능 여부 확인
    /// </summary>
    private bool CanTeleport()
    {
        if (teleportTarget == null) return false;
        
        // 공유 쿨타임 확인
        if (TeleportCooldownManager.Instance != null && !TeleportCooldownManager.Instance.CanTeleport())
        {
            float remainingCooldown = TeleportCooldownManager.Instance.GetRemainingCooldown();
            Debug.Log($"텔레포트 쿨타임 중입니다! {remainingCooldown:F1}초 남음");
            return false;
        }
        
        // 광산 상태 확인
        if (checkMineActive && requiredMine != null)
        {
            if (!requiredMine.IsBuilt)
            {
                Debug.Log("텔레포트 불가: 광산이 건설되지 않았거나 비활성화 상태입니다.");
                return false;
            }
        }
        
        // 비용이 설정되어 있다면 확인
        if (requiredMineral != null && requiredAmount > 0)
        {
            return ResourceManager.Instance.HasEnoughResource(requiredMineral, requiredAmount);
        }
        
        return true;
    }
    
    /// <summary>
    /// 텔레포트 시퀀스 (공유 쿨타임 사용)
    /// </summary>
    private IEnumerator TeleportSequence()
    {
        // 즉시 텔레포트 상태로 전환
        isTeleporting = true;
        if (TeleportCooldownManager.Instance != null)
        {
            TeleportCooldownManager.Instance.StartTeleport(); // 공유 쿨타임 적용
        }
        
        Debug.Log("=== 광산 텔레포트 시작! 공유 쿨타임 적용 ===");
        
        // 모든 입력과 이동 즉시 차단
        Input.ResetInputAxes();
        DisablePlayerControl();
        
        // 비용 소모
        if (requiredMineral != null && requiredAmount > 0)
        {
            ResourceManager.Instance.UseResource(requiredMineral, requiredAmount);
            Debug.Log($"{requiredMineral.mineralName} {requiredAmount}개 소모!");
        }
        
        // 페이드 아웃 (화면이 검게 변함)
        yield return StartCoroutine(FadeScreen(true));
        
        // 플레이어 이동 및 물리 상태 초기화
        if (player != null && teleportTarget != null)
        {
            // 위치 이동
            Vector3 targetPos = new Vector3(teleportTarget.position.x, teleportTarget.position.y, 0);
            player.transform.position = targetPos;
            Debug.Log($"플레이어를 {targetPos}로 이동!");
            
            // 그라운드 매니저를 통한 카메라 Y 위치 자동 조정
            if (GroundManager.Instance != null)
            {
                GroundManager.Instance.AdjustCameraForPlayerPosition(teleportTarget.position.y);
            }
            
            // 이동 후 물리 상태 완전 초기화
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                Debug.Log("이동 후 물리 상태 완전 초기화");
            }
        }
        // 텔레포트 후 현재 위치의 안전지대 상태를 즉시 확인합니다.
        DangerGaugeSystem dangerSystem = player.GetComponent<DangerGaugeSystem>();
        if (dangerSystem != null)
        {
            dangerSystem.IsPlayerInSafeZone();
        }
        
        // 페이드 인 (화면이 다시 보임)
        yield return StartCoroutine(FadeScreen(false));
        
        // 플레이어 제어 다시 활성화
        EnablePlayerControl();
        
        // 텔레포트 완료, 공유 쿨타임은 이미 시작됨
        isTeleporting = false;

        Debug.Log($"=== 광산 텔레포트 완료! 공유 쿨타임 진행 중 ===");
    }
    
    /// <summary>
    /// 화면 페이드 효과
    /// </summary>
    private IEnumerator FadeScreen(bool fadeOut)
    {
        Debug.Log($"페이드 시작: {(fadeOut ? "아웃" : "인")}");
        
        // UI Canvas 찾기 또는 생성
        Canvas fadeCanvas = FindOrCreateFadeCanvas();
        UnityEngine.UI.Image fadeImage = GetFadeImage(fadeCanvas);
        
        // 이미지 활성화
        fadeImage.gameObject.SetActive(true);
        
        float startAlpha = fadeOut ? 0f : 1f; // 시작 투명도
        float targetAlpha = fadeOut ? 1f : 0f; // 목표 투명도
        
        Color color = fadeColor;
        color.a = startAlpha;
        fadeImage.color = color;
        
        Debug.Log($"페이드 애니메이션 시작: {startAlpha} → {targetAlpha}, 예상 시간: {fadeTime}초");
        
        // 정확한 페이드 애니메이션 (Time.unscaledDeltaTime 사용)
        float elapsedTime = 0f;
        float startTime = Time.unscaledTime;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime = Time.unscaledTime - startTime; // 정확한 경과 시간 계산
            float progress = Mathf.Clamp01(elapsedTime / fadeTime); // 0~1 사이 진행률
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }
        
        // 최종 색상 설정 (정확히 목표값으로)
        color.a = targetAlpha;
        fadeImage.color = color;
        
        Debug.Log($"페이드 완료: 최종 알파 = {targetAlpha}, 실제 소요 시간: {Time.unscaledTime - startTime:F3}초");
        
        // 페이드 인 완료 시에만 이미지 숨김
        if (!fadeOut)
        {
            fadeImage.gameObject.SetActive(false);
            Debug.Log("페이드 이미지 비활성화");
        }
    }
    
    /// <summary>
    /// 페이드용 Canvas 찾기 또는 생성
    /// </summary>
    private Canvas FindOrCreateFadeCanvas()
    {
        Canvas canvas = GameObject.Find("FadeCanvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            // 새로운 Canvas 생성
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 최상위에 그리기
            
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        return canvas;
    }
    
    /// <summary>
    /// 페이드용 이미지 가져오기
    /// </summary>
    private UnityEngine.UI.Image GetFadeImage(Canvas canvas)
    {
        Transform fadeImageTransform = canvas.transform.Find("FadeImage");
        if (fadeImageTransform == null)
        {
            // 새로운 이미지 생성
            GameObject fadeImageObj = new GameObject("FadeImage");
            fadeImageTransform = fadeImageObj.transform;
            fadeImageTransform.SetParent(canvas.transform, false);
            
            UnityEngine.UI.Image image = fadeImageObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            
            // 전체 화면 크기로 설정
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Debug.Log("새로운 페이드 이미지 생성됨");
            return image;
        }
        
        UnityEngine.UI.Image existingImage = fadeImageTransform.GetComponent<UnityEngine.UI.Image>();
        Debug.Log("기존 페이드 이미지 반환됨");
        return existingImage;
    }
    
    /// <summary>
    /// 플레이어가 범위에 들어옴
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // 공유 쿨타임 확인
            if (TeleportCooldownManager.Instance != null && !TeleportCooldownManager.Instance.CanTeleport())
            {
                float remainingCooldown = TeleportCooldownManager.Instance.GetRemainingCooldown();
                Debug.Log($"광산 텔레포트 범위 진입! (공유 쿨타임 {remainingCooldown:F1}초 남음)");
            }
            else
            {
                Debug.Log($"[W] 키를 눌러 광산 텔레포트 (범위 진입)");
            }
        }
    }
    
    /// <summary>
    /// 플레이어가 범위에서 나감
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("텔레포트 범위 이탈");
        }
    }
    
    /// <summary>
    /// 플레이어 제어 비활성화 (InputManager TestDisable 사용)
    /// </summary>
    private void DisablePlayerControl()
    {
        if (player == null) return;
        
        Debug.Log("=== 플레이어 제어 완전 비활성화 시작 ===");
        
        // MovementLimiter를 통해 이동을 막습니다.
        if (MovementLimiter.Instance != null)
        {
            MovementLimiter.Instance.SetCanMove(false);
            Debug.Log("MovementLimiter를 통해 이동 비활성화");
        }
        
        // Rigidbody2D 완전 정지
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Debug.Log("Rigidbody2D 완전 정지");
        }
        
        // CharacterMove 상태 초기화
        CharacterMove characterMove = player.GetComponent<CharacterMove>();
        if (characterMove != null) 
        {
            characterMove.directionX = 0f;
            characterMove.pressingKey = false;
            characterMove.velocity = Vector2.zero;
            Debug.Log("CharacterMove 상태 초기화");
        }
        
        Debug.Log("=== 플레이어 제어 완전 비활성화 완료 ===");
    }
    
    /// <summary>
    /// 플레이어 제어 활성화 (InputManager TestAble 사용)
    /// </summary>
    private void EnablePlayerControl()
    {
        if (player == null) return;
        
        Debug.Log("=== 플레이어 제어 완전 활성화 시작 ===");
        
        // 짧은 대기 후 제어 복구
        StartCoroutine(DelayedControlRestore());
    }
    
    /// <summary>
    /// 지연된 제어 복구 (물리 시스템 안정화 후)
    /// </summary>
    private IEnumerator DelayedControlRestore()
    {
        yield return new WaitForSeconds(0.1f); // 최소한의 대기로 변경

        // 플레이어가 죽은 상태라면 제어를 복구하지 않고 코루틴을 즉시 종료합니다.
        PlayerStatus status = player.GetComponent<PlayerStatus>();
        if (status != null && status.IsDead)
        {
            Debug.Log("플레이어가 사망하여 제어 복구를 중단합니다.");
            yield break;
        }
        
        // CharacterMove 상태 완전 초기화
        CharacterMove characterMove = player.GetComponent<CharacterMove>();
        if (characterMove != null) 
        {
            characterMove.directionX = 0f;
            characterMove.pressingKey = false;
            characterMove.velocity = Vector2.zero;
            
            // 스프라이트 방향을 오른쪽으로 설정
            player.transform.localScale = new Vector3(
                Mathf.Abs(player.transform.localScale.x), 
                player.transform.localScale.y, 
                player.transform.localScale.z
            );
            
            Debug.Log("CharacterMove 상태 완전 초기화 및 스프라이트 오른쪽 설정");
        }
        
        // Rigidbody2D 완전 초기화
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            Debug.Log("Rigidbody2D 완전 초기화");
        }
        
        // InputManager의 TestAble 메서드 사용 (전역 입력 활성화)
        if (InputManager.Instance != null)
        {
            // InputManager.Instance.TestAble();
            // Debug.Log("InputManager.TestAble() 호출");
        }
        
        MovementLimiter.Instance?.SetCanMove(true);
        Debug.Log("=== 플레이어 제어 완전 활성화 완료 (MovementLimiter) ===");
    }
    
    /// <summary>
    /// 범위 표시 (Scene 뷰에서)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // 목표 지점 표시
        if (teleportTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(teleportTarget.position, 1f);
            Gizmos.DrawLine(transform.position, teleportTarget.position);
        }
    }
}