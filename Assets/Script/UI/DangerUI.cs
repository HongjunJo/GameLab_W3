using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    [SerializeField] private Color mediumDangerColor = new Color(1f, 0.5f, 0f); // 주황색
    [SerializeField] private Color highDangerColor = Color.red; // 빨간색
    private Coroutine flashCoroutine;
    
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
            dangerText.text = "O2: 100/100";
        }
        
        if (dangerSlider != null)
        {
            dangerSlider.interactable = false; // 슬라이더 클릭 방지
            dangerSlider.value = 1f; // 시작 시 슬라이더를 가득 채움
        }
        
        // 산소 시스템에 맞게 색상 로직을 반대로 적용
        UpdateFillColor(1f);
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
            dangerText.text = $"O2: {displayValue:F0}/{maximum:F0}";
        }
        
        // 슬라이더 업데이트
        if (dangerSlider != null)
        {
            dangerSlider.value = maximum > 0 ? Mathf.Min(current, maximum) / maximum : 0;
        }
        
        // 산소량에 따른 색상 변경
        float oxygenRatio = maximum > 0 ? current / maximum : 0;
        UpdateFillColor(oxygenRatio);
        
        // 산소량에 따른 깜빡임 효과 처리
        HandleFlashingEffect(oxygenRatio);
    }
    
    /// <summary>
    /// 산소량에 따른 게이지 색상 업데이트 (로직 반전)
    /// </summary>
    private void UpdateFillColor(float oxygenRatio)
    {
        if (dangerFill == null) return;
        
        Color targetColor;
        
        if (oxygenRatio > 0.5f) // 100-50%: 안전 (초록)
        {
            // 100%일 때 초록색, 50%일 때 노란색으로 점진적 변경
            targetColor = Color.Lerp(mediumDangerColor, lowDangerColor, (oxygenRatio - 0.5f) * 2f);
        }
        else if (oxygenRatio > 0.25f) // 50-25%: 주의 (주황)
        {
            targetColor = Color.Lerp(highDangerColor, mediumDangerColor, (oxygenRatio - 0.25f) * 4f);
        }
        else // 25-0%: 위험 (빨강)
        {
            targetColor = highDangerColor;
        }
        
        dangerFill.color = targetColor;
    }
    
    /// <summary>
    /// 산소량에 따른 깜빡임 효과 처리
    /// </summary>
    private void HandleFlashingEffect(float oxygenRatio)
    {
        if (oxygenRatio < 0.25f) // 산소가 25% 미만일 때 깜빡임 시작
        {
            if (flashCoroutine == null)
                flashCoroutine = StartCoroutine(FlashEffect());
        }
        else // 산소가 25% 이상이면 깜빡임 중지
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
                Color c = dangerFill.color;
                c.a = 1f; // 알파값 복원
                dangerFill.color = c;
            }
        }
    }

    /// <summary>
    /// 게이지 깜빡임 효과 코루틴
    /// </summary>
    private IEnumerator FlashEffect()
    {
        while (true)
        {
            if (dangerFill == null) yield break; // Null 체크 추가

            float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * 8f); // 빠른 깜빡임
            Color currentColor = dangerFill.color;
            currentColor.a = alpha;
            dangerFill.color = currentColor;
            yield return null;
        }
    }
}