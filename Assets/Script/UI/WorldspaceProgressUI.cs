using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 월드 스페이스에 위치한 진행률 표시 UI(슬라이더)를 관리합니다.
/// UI가 항상 카메라를 바라보도록(빌보드 효과) 처리합니다.
/// </summary>
public class WorldspaceProgressUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider progressSlider;

    private Camera mainCamera;

    private void Awake()
    {
        // 슬라이더가 없으면 자식에서 찾아 할당합니다.
        if (progressSlider == null)
        {
            progressSlider = GetComponentInChildren<Slider>();
        }
        if (progressSlider == null)
        {
            Debug.LogError("Progress Slider가 할당되지 않았습니다.", this);
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        Hide(); // 시작 시 숨김
    }

    private void LateUpdate()
    {
        // 빌보드 효과: UI가 항상 메인 카메라를 바라보도록 회전
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    /// <summary>
    /// 진행률을 업데이트합니다. (0.0 ~ 1.0 사이의 값)
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }
    }
}
