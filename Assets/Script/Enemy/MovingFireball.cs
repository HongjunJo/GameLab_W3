using UnityEngine;
using System.Collections;

/// <summary>
/// 포물선 궤적을 그리며 움직이는 파이어볼 스크립트
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class MovingFireball : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("파이어볼이 도달할 최대 높이입니다.")]
    [SerializeField] private float maxHeight = 5f;

    [Tooltip("시작 위치로부터 착지할 목표 지점까지의 X축 거리입니다. (음수 입력 시 왼쪽으로)")]
    [SerializeField] private float targetXOffset = 10f;

    [Tooltip("초기 위치로 돌아온 후 다시 튀어오르기까지 대기하는 시간(초)입니다.")]
    [SerializeField] private float waitTime = 2f;

    [Tooltip("파이어볼의 전체적인 속도 배율입니다. 값을 높이면 더 빠르고 역동적으로 움직입니다.")]
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("처음 발사되기 전까지 대기하는 시간(초)입니다. 여러 개를 엇갈리게 발사할 때 사용합니다.")]
    [SerializeField] private float initialDelay = 0f;

    private Rigidbody2D rb;
    private Vector3 startPosition;
    private bool isWaiting = false;

    /// <summary>
    /// 파이어볼의 초기 위치를 외부에서 설정할 수 있게 합니다.
    /// </summary>
    /// <param name="pos">시작 위치</param>
    private void Awake()
    {
        // Rigidbody 컴포넌트를 가져옵니다. 3D 오브젝트이므로 Rigidbody를 사용합니다.
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D 컴포넌트가 없습니다. MovingFireball은 Rigidbody2D가 필요합니다.");
            return;
        }

        // 2D 횡스크롤 게임이므로 Y축 이외의 회전과 Z축 이동을 제한합니다.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // 2D에서는 Z축 이동이 기본적으로 제한됩니다.

        // 초기 위치를 저장합니다.
        startPosition = transform.position;
    }

    private void Start()
    {
        // 초기 지연 시간 후 첫 발사를 시작합니다.
        StartCoroutine(InitialLaunch());
    }

    private void Update()
    {
        // 대기 중이 아닐 때, 파이어볼이 아래로 떨어지고 있고, 시작 높이보다 낮거나 같아지면 리셋 로직을 시작합니다.
        if (!isWaiting && rb.linearVelocity.y < 0 && transform.position.y <= startPosition.y)
        {
            StartCoroutine(ResetAndRelaunch());
        }
    }
    
    /// <summary>
    /// 파이어볼의 위치를 초기화하고 다시 발사하는 코루틴입니다.
    /// </summary>
    private IEnumerator ResetAndRelaunch()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero; // 물리적 움직임을 즉시 멈춥니다.
        rb.gravityScale = 0; // 중력의 영향을 받지 않도록 합니다.
        transform.position = new Vector3(startPosition.x, startPosition.y, startPosition.z); // X, Y 좌표를 초기 위치로 리셋합니다.

        yield return new WaitForSeconds(waitTime); // 설정된 시간만큼 대기합니다.
        
        rb.gravityScale = 1; // 중력을 다시 활성화합니다.
        Launch(); // 다시 발사합니다.
        isWaiting = false;
    }

    /// <summary>
    /// 초기 지연 후 첫 발사를 처리하는 코루틴입니다.
    /// </summary>
    private IEnumerator InitialLaunch()
    {
        rb.gravityScale = 0; // 초기 대기 동안 중력 비활성화
        isWaiting = true; // 첫 발사 전까지는 대기 상태로 간주
        yield return new WaitForSeconds(initialDelay);
        rb.gravityScale = 1; // 발사 직전 중력 다시 활성화
        isWaiting = false;
        Launch();
    }
    /// <summary>
    /// 파이어볼을 발사합니다.
    /// </summary>
    private void Launch()
    {
        float gravity = Physics2D.gravity.y * rb.gravityScale; // 2D 물리 중력 사용

        // 1. 최대 높이에 도달하기 위한 초기 수직 속도(velocityY) 계산
        // 물리 공식: v_y^2 = v_y0^2 + 2 * a * d  =>  0 = v_y0^2 + 2 * (-g) * h  =>  v_y0 = sqrt(2 * g * h)
        float velocityY = Mathf.Sqrt(-2 * gravity * maxHeight) * speedMultiplier;

        // 2. 최고점까지 올라갔다 내려오는 총 비행 시간(timeToTarget) 계산
        // 올라가는 시간(t_up) = v_y0 / g. 총 시간은 t_up * 2
        float timeToTarget = (2 * velocityY) / -gravity;

        // 3. 총 비행 시간 동안 목표 X좌표까지 도달하기 위한 수평 속도(velocityX) 계산
        // 속도 배율이 적용된 수직 속도로 비행 시간이 재계산되었으므로, 수평 속도도 자연스럽게 빨라집니다.
        float velocityX = targetXOffset / timeToTarget;

        // 계산된 힘과 수평 속도를 적용합니다.
        // AddForce 대신 linearVelocity에 직접 할당하여 질량과 관계없이 정확한 속도를 부여합니다.
        rb.linearVelocity = new Vector2(velocityX, velocityY);

        // Debug.Log($"Fireball launched with velocity: {rb.linearVelocity}");
    }

    /// <summary>
    /// 에디터에서 예상 궤적을 기즈모로 그립니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (maxHeight <= 0) return;

        // 시작 위치 설정 (에디터에서는 현재 위치, 플레이 중에는 저장된 시작 위치)
        Vector3 gizmoStartPosition = Application.isPlaying ? startPosition : transform.position;

        // Awake에서 캐시된 Rigidbody2D를 사용하는 것이 더 안전하고 효율적입니다.
        // rb가 null일 경우를 대비하여 기본값 1f를 사용합니다.
        float currentGravityScale = (rb != null) ? rb.gravityScale : 1f;
        float gravity = Physics2D.gravity.y * currentGravityScale;
        float velocityY = Mathf.Sqrt(-2 * gravity * maxHeight) * speedMultiplier;
        float timeToTarget = (2 * velocityY) / -gravity;
        float velocityX = targetXOffset / timeToTarget;

        Gizmos.color = Color.red;

        Vector3 previousPoint = gizmoStartPosition;
        int resolution = 30; // 궤적을 그릴 때 사용할 점의 개수

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / resolution * timeToTarget;
            float x = velocityX * t;
            float y = velocityY * t + 0.5f * gravity * t * t;
            Vector3 currentPoint = gizmoStartPosition + new Vector3(x, y, 0);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
