using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 위험도 게이지 UI 표시 컴포넌트
/// </summary>
public class DangerUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI dangerText;
    [SerializeField] private Slider dangerSlider;
    [SerializeField] private Image dangerFill;
    
    [Header("Visual Settings")]
    [SerializeField] private Color lowDangerColor = Color.green;
    [SerializeField] private Color mediumDangerColor = Color.yellow;
    [SerializeField] private Color highDangerColor = Color.red;
    [SerializeField] private Color criticalDangerColor = new Color(0.8f, 0f, 0f, 1f); // 진한 빨강
    
    private void OnEnable()
    {
        GameEvents.OnDangerChanged += UpdateDisplay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnDangerChanged -= UpdateDisplay;
    }
    
    private void Start()
    {
        // 초기 표시
        if (dangerText != null)
        {
            dangerText.text = "Danger: 0/100";
        }
        
        if (dangerSlider != null)
        {
            dangerSlider.value = 0f;
        }
        
        UpdateFillColor(0f);
    }
    
    /// <summary>
    /// 위험도 표시 업데이트
    /// </summary>
    private void UpdateDisplay(float current, float maximum)
    {
        // 텍스트 업데이트 (UI에서는 100으로 클램프된 값 표시)
        if (dangerText != null)
        {
            float displayValue = Mathf.Min(current, maximum);
            dangerText.text = $"Danger: {displayValue:F0}/{maximum:F0}";
        }
        
        // 슬라이더 업데이트
        if (dangerSlider != null)
        {
            dangerSlider.value = maximum > 0 ? Mathf.Min(current, maximum) / maximum : 0;
        }
        
        // 위험도에 따른 색상 변경
        float dangerRatio = maximum > 0 ? current / maximum : 0;
        UpdateFillColor(dangerRatio);
    }
    
    /// <summary>
    /// 위험도에 따른 게이지 색상 업데이트
    /// </summary>
    private void UpdateFillColor(float dangerRatio)
    {
        if (dangerFill == null) return;
        
        Color targetColor;
        
        if (dangerRatio < 0.25f) // 0-25%: 안전 (초록)
        {
            targetColor = lowDangerColor;
        }
        else if (dangerRatio < 0.5f) // 25-50%: 주의 (노랑)
        {
            targetColor = Color.Lerp(lowDangerColor, mediumDangerColor, (dangerRatio - 0.25f) * 4f);
        }
        else if (dangerRatio < 0.75f) // 50-75%: 위험 (주황)
        {
            targetColor = Color.Lerp(mediumDangerColor, highDangerColor, (dangerRatio - 0.5f) * 4f);
        }
        else // 75-100%: 극도 위험 (빨강)
        {
            targetColor = Color.Lerp(highDangerColor, criticalDangerColor, (dangerRatio - 0.75f) * 4f);
        }
        
        dangerFill.color = targetColor;
    }
    
    /// <summary>
    /// 위험도 게이지 깜빡임 효과 (높은 위험도일 때)
    /// </summary>
    private void Update()
    {
        if (dangerFill != null && dangerSlider != null)
        {
            // 90% 이상일 때 깜빡임 효과
            if (dangerSlider.value >= 0.9f)
            {
                float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * 8f); // 빠른 깜빡임
                Color currentColor = dangerFill.color;
                currentColor.a = alpha;
                dangerFill.color = currentColor;
            }
            else
            {
                // 정상 알파 값 복원
                Color currentColor = dangerFill.color;
                currentColor.a = 1f;
                dangerFill.color = currentColor;
            }
        }
    }
}