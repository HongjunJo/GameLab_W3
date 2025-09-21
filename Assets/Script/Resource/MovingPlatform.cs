using UnityEngine;
using System.Collections;

/// <summary>
/// 지정된 방향과 거리만큼 왕복 운동하는 플랫폼 스크립트
/// </summary>
public class MovingPlatform : MonoBehaviour
{
    /// <summary>
    /// 이동 방향을 정의합니다.
    /// </summary>
    public enum MoveDirection
    {
        Horizontal, // 좌우
        Vertical    // 상하
    }

    [Header("Movement Settings")]
    [Tooltip("플랫폼이 움직일 방향을 선택합니다.")]
    [SerializeField] private MoveDirection direction = MoveDirection.Horizontal;

    [Tooltip("플랫폼이 이동할 거리입니다.")]
    [SerializeField] private float moveDistance = 5f;

    [Tooltip("플랫폼의 이동 속도입니다.")]
    [SerializeField] private float speed = 2f;

    [Tooltip("플랫폼이 목표 지점에서 대기할 시간입니다.")]
    [SerializeField] private float waitTime = 1f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMovingToTarget = true;
    private Vector3 lastPosition; // 이전 프레임의 위치를 저장할 변수
    private Rigidbody2D playerRigidbody; // 2D 플레이어 리지드바디

    private void Start()
    {
        // 초기 위치와 목표 위치를 설정합니다.
        startPosition = transform.position;
        CalculateTargetPosition();
        
        // 코루틴을 시작하여 플랫폼을 움직입니다.
        StartCoroutine(MovePlatform());

        // FixedUpdate에서 위치 변화량을 계산하기 위해 초기화
        lastPosition = transform.position;
    }

    /// <summary>
    /// 이동 방향에 따라 목표 위치를 계산합니다.
    /// </summary>
    private void CalculateTargetPosition()
    {
        switch (direction)
        {
            case MoveDirection.Horizontal:
                targetPosition = startPosition + Vector3.right * moveDistance;
                break;
            case MoveDirection.Vertical:
                targetPosition = startPosition + Vector3.up * moveDistance;
                break;
        }
    }

    /// <summary>
    /// 플랫폼을 왕복 이동시키는 코루틴입니다.
    /// </summary>
    private IEnumerator MovePlatform()
    {
        while (true) // 무한 반복
        {
            // 현재 목표 지점을 설정합니다.
            Vector3 currentTarget = isMovingToTarget ? targetPosition : startPosition;
            
            // 목표 지점에 도달할 때까지 프레임마다 이동합니다.
            while (Vector3.Distance(transform.position, currentTarget) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
                yield return null; // 다음 프레임까지 대기
            }

            // 목표 지점에 정확히 위치시킵니다.
            transform.position = currentTarget;

            // 목표 지점에서 잠시 대기합니다.
            yield return new WaitForSeconds(waitTime);

            // 다음 목표 지점을 위해 방향을 전환합니다.
            isMovingToTarget = !isMovingToTarget;
        }
    }

    private void FixedUpdate()
    {
        // 현재 프레임과 이전 프레임의 위치 차이를 계산
        Vector3 velocity = transform.position - lastPosition;

        // 플레이어가 발판 위에 있을 때만 위치를 보정
        if (playerRigidbody != null)
        {
            playerRigidbody.transform.position += velocity;
        }

        // 다음 프레임을 위해 현재 위치를 저장
        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision) // 2D
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어가 위에서 착지했을 때만
            if (collision.contacts[0].normal.y < -0.5)
                playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private void OnCollisionExit2D(Collision2D collision) // 2D
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRigidbody = null;
        }
    }

    // 3D 환경에서는 Rigidbody를 사용해야 합니다.
    private void OnCollisionEnter(Collision collision) // 3D
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 3D에서는 Rigidbody를 사용하므로 이 방식은 적합하지 않습니다.
            // 3D의 경우, 플레이어 Rigidbody에 직접 속도를 더해주는 방식이 더 안정적입니다.
            // 지금은 2D 환경에 집중하여 수정했습니다.
        }
    }

    /// <summary>
    /// 에디터의 Scene 뷰에서 이동 경로를 시각적으로 보여줍니다.
    /// </summary>
    private void OnDrawGizmos()
    {
        // 기즈모가 그려질 때 아직 startPosition이 설정되지 않았다면 현재 위치를 사용합니다.
        Vector3 gizmoStart = Application.isPlaying ? startPosition : transform.position;
        Vector3 gizmoTarget = gizmoStart;

        // 이동 방향에 따라 목표 위치를 계산합니다.
        switch (direction)
        {
            case MoveDirection.Horizontal:
                gizmoTarget += Vector3.right * moveDistance;
                break;
            case MoveDirection.Vertical:
                gizmoTarget += Vector3.up * moveDistance;
                break;
        }

        // 이동 경로를 선으로 그립니다.
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(gizmoStart, gizmoTarget);

        // 시작점과 끝점을 큐브로 표시합니다.
        Gizmos.DrawWireCube(gizmoStart, Vector3.one * 0.5f);
        Gizmos.DrawWireCube(gizmoTarget, Vector3.one * 0.5f);
    }
}
