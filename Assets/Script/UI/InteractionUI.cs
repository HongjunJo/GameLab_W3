using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상호작용 프롬프트 UI를 관리하는 컴포넌트
/// </summary>
public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private Text interactionText;
    [SerializeField] private Text keyPromptText;
    [SerializeField] private Image interactionIcon;
    
    [Header("Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private bool autoHideWhenNoInteraction = true;
    [SerializeField] private float fadeSpeed = 3f;
    
    [Header("Visual Effects")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private float baseFontSize;
    private bool isVisible = false;
    
    private void Awake()
    {
        // CanvasGroup 컴포넌트 확인/추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 기본 폰트 크기 저장
        if (interactionText != null)
        {
            baseFontSize = interactionText.fontSize;
        }
        
        // 초기 상태 설정
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
        
        // 키 프롬프트 텍스트 설정
        UpdateKeyPromptText();
    }
    
    private void Update()
    {
        // 페이드 효과 처리
        if (Mathf.Abs(canvasGroup.alpha - targetAlpha) > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
        
        // 펄스 효과 처리
        if (enablePulseEffect && isVisible && interactionText != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            interactionText.fontSize = Mathf.RoundToInt(baseFontSize * (1f + pulse));
        }
    }
    
    /// <summary>
    /// 상호작용 UI 표시
    /// </summary>
    public void ShowInteraction(string message, Sprite icon = null)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        isVisible = true;
        targetAlpha = 1f;
        
        // 패널 활성화
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
        }
        
        // 텍스트 설정
        if (interactionText != null)
        {
            interactionText.text = message;
        }
        
        // 아이콘 설정
        if (interactionIcon != null)
        {
            if (icon != null)
            {
                interactionIcon.sprite = icon;
                interactionIcon.gameObject.SetActive(true);
            }
            else
            {
                interactionIcon.gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"Showing interaction UI: {message}");
    }
    
    /// <summary>
    /// 상호작용 UI 숨기기
    /// </summary>
    public void HideInteraction()
    {
        isVisible = false;
        targetAlpha = 0f;
        
        // 애니메이션 완료 후 패널 비활성화
        if (autoHideWhenNoInteraction)
        {
            Invoke(nameof(DisablePanel), 1f / fadeSpeed);
        }
        
        Debug.Log("Hiding interaction UI");
    }
    
    /// <summary>
    /// 패널 비활성화 (지연 호출용)
    /// </summary>
    private void DisablePanel()
    {
        if (!isVisible && interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// 상호작용 키 변경
    /// </summary>
    public void SetInteractionKey(KeyCode newKey)
    {
        interactionKey = newKey;
        UpdateKeyPromptText();
    }
    
    /// <summary>
    /// 키 프롬프트 텍스트 업데이트
    /// </summary>
    private void UpdateKeyPromptText()
    {
        if (keyPromptText != null)
        {
            keyPromptText.text = $"Press [{interactionKey}]";
        }
    }
    
    /// <summary>
    /// 즉시 표시/숨기기 (애니메이션 없이)
    /// </summary>
    public void SetVisibilityImmediate(bool visible)
    {
        isVisible = visible;
        targetAlpha = visible ? 1f : 0f;
        canvasGroup.alpha = targetAlpha;
        
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(visible);
        }
    }
    
    /// <summary>
    /// 펄스 효과 활성화/비활성화
    /// </summary>
    public void SetPulseEffect(bool enabled)
    {
        enablePulseEffect = enabled;
        
        // 펄스 효과 비활성화시 원래 크기로 복원
        if (!enabled && interactionText != null)
        {
            interactionText.fontSize = Mathf.RoundToInt(baseFontSize);
        }
    }
    
    /// <summary>
    /// 현재 표시 상태 확인
    /// </summary>
    public bool IsVisible()
    {
        return isVisible;
    }
    
    /// <summary>
    /// 테스트용 상호작용 표시
    /// </summary>
    [ContextMenu("Test Show Interaction")]
    public void TestShowInteraction()
    {
        ShowInteraction("Test Interaction Message");
    }
    
    /// <summary>
    /// 테스트용 상호작용 숨기기
    /// </summary>
    [ContextMenu("Test Hide Interaction")]
    public void TestHideInteraction()
    {
        HideInteraction();
    }
}