using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전력 UI를 표시하고 관리하는 컴포넌트
/// </summary>
public class PowerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Text powerText;
    [SerializeField] private Text powerPercentageText;
    [SerializeField] private Image powerFillImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color fullPowerColor = Color.green;
    [SerializeField] private Color mediumPowerColor = Color.yellow;
    [SerializeField] private Color lowPowerColor = Color.red;
    [SerializeField] private float lowPowerThreshold = 0.25f;
    [SerializeField] private float mediumPowerThreshold = 0.6f;
    
    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 2f;
    
    private float targetValue = 1f;
    private float currentDisplayValue = 1f;
    
    private void OnEnable()
    {
        GameEvents.OnPowerChanged += UpdatePowerDisplay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPowerChanged -= UpdatePowerDisplay;
    }
    
    private void Start()
    {
        // 초기 전력 표시
        if (PowerManager.Instance != null)
        {
            UpdatePowerDisplay(PowerManager.Instance.CurrentPower, PowerManager.Instance.MaxPower);
        }
    }
    
    private void Update()
    {
        // 애니메이션 처리
        if (animateChanges && Mathf.Abs(currentDisplayValue - targetValue) > 0.01f)
        {
            currentDisplayValue = Mathf.Lerp(currentDisplayValue, targetValue, Time.deltaTime * animationSpeed);
            UpdateSliderValue(currentDisplayValue);
        }
    }
    
    /// <summary>
    /// 전력 표시 업데이트
    /// </summary>
    private void UpdatePowerDisplay(float currentPower, float maxPower)
    {
        float powerRatio = maxPower > 0 ? currentPower / maxPower : 0f;
        
        if (animateChanges)
        {
            targetValue = powerRatio;
        }
        else
        {
            currentDisplayValue = powerRatio;
            UpdateSliderValue(powerRatio);
        }
        
        // 텍스트 업데이트
        UpdatePowerText(currentPower, maxPower);
        
        // 색상 업데이트
        UpdatePowerColor(powerRatio);
    }
    
    /// <summary>
    /// 슬라이더 값 업데이트
    /// </summary>
    private void UpdateSliderValue(float ratio)
    {
        if (powerSlider != null)
        {
            powerSlider.value = ratio;
        }
    }
    
    /// <summary>
    /// 전력 텍스트 업데이트
    /// </summary>
    private void UpdatePowerText(float currentPower, float maxPower)
    {
        // 기본 전력 텍스트
        if (powerText != null)
        {
            powerText.text = $"Power: {currentPower:F0}/{maxPower:F0}";
        }
        
        // 퍼센티지 텍스트
        if (powerPercentageText != null)
        {
            float percentage = maxPower > 0 ? (currentPower / maxPower) * 100f : 0f;
            powerPercentageText.text = $"{percentage:F0}%";
        }
    }
    
    /// <summary>
    /// 전력 색상 업데이트
    /// </summary>
    private void UpdatePowerColor(float powerRatio)
    {
        Color targetColor;
        
        if (powerRatio <= lowPowerThreshold)
        {
            targetColor = lowPowerColor;
        }
        else if (powerRatio <= mediumPowerThreshold)
        {
            targetColor = mediumPowerColor;
        }
        else
        {
            targetColor = fullPowerColor;
        }
        
        // 슬라이더 채우기 색상 변경
        if (powerFillImage != null)
        {
            powerFillImage.color = targetColor;
        }
        
        // 텍스트 색상도 변경
        if (powerText != null)
        {
            powerText.color = targetColor;
        }
    }
    
    /// <summary>
    /// 전력 부족 경고 표시
    /// </summary>
    public void ShowLowPowerWarning()
    {
        // 깜빡이는 효과나 다른 경고 UI를 여기에 구현
        Debug.Log("Low Power Warning!");
    }
    
    /// <summary>
    /// 수동으로 UI 업데이트
    /// </summary>
    [ContextMenu("Update Power Display")]
    public void ForceUpdateDisplay()
    {
        if (PowerManager.Instance != null)
        {
            UpdatePowerDisplay(PowerManager.Instance.CurrentPower, PowerManager.Instance.MaxPower);
        }
    }
    
    /// <summary>
    /// 애니메이션 설정 변경
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        animateChanges = enabled;
    }
    
    /// <summary>
    /// 애니메이션 속도 설정
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = speed;
    }
}