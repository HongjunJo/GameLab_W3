using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 체력 UI를 표시하고 관리하는 컴포넌트
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private GameObject lowHealthWarning;
    
    [Header("Visual Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    
    [Header("Warning Settings")]
    [SerializeField] private bool enableLowHealthWarning = true;
    [SerializeField] private float warningBlinkSpeed = 2f;
    
    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 3f;
    
    private float targetValue = 1f;
    private float currentDisplayValue = 1f;
    private bool isBlinking = false;
    
    private void OnEnable()
    {
        GameEvents.OnHealthChanged += UpdateHealthDisplay;
        GameEvents.OnPlayerDied += OnPlayerDied;
    }
    
    private void OnDisable()
    {
        GameEvents.OnHealthChanged -= UpdateHealthDisplay;
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }
    
    private void Start()
    {
        // 초기 체력 표시 - 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                UpdateHealthDisplay(playerHealth.CurrentHP, playerHealth.MaxHP);
            }
        }
        
        // 경고 UI 초기화
        if (lowHealthWarning != null)
        {
            lowHealthWarning.SetActive(false);
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
        
        // 저체력 경고 깜빡임
        if (isBlinking && lowHealthWarning != null)
        {
            float alpha = (Mathf.Sin(Time.time * warningBlinkSpeed) + 1f) * 0.5f;
            CanvasGroup canvasGroup = lowHealthWarning.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }
        }
    }
    
    /// <summary>
    /// 체력 표시 업데이트
    /// </summary>
    private void UpdateHealthDisplay(float currentHP, float maxHP)
    {
        float healthRatio = maxHP > 0 ? currentHP / maxHP : 0f;
        
        if (animateChanges)
        {
            targetValue = healthRatio;
        }
        else
        {
            currentDisplayValue = healthRatio;
            UpdateSliderValue(healthRatio);
        }
        
        // 텍스트 업데이트
        UpdateHealthText(currentHP, maxHP);
        
        // 색상 업데이트
        UpdateHealthColor(healthRatio);
        
        // 저체력 경고 처리
        HandleLowHealthWarning(healthRatio);
    }
    
    /// <summary>
    /// 슬라이더 값 업데이트
    /// </summary>
    private void UpdateSliderValue(float ratio)
    {
        if (healthSlider != null)
        {
            healthSlider.value = ratio;
        }
    }
    
    /// <summary>
    /// 체력 텍스트 업데이트
    /// </summary>
    private void UpdateHealthText(float currentHP, float maxHP)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHP:F0}/{maxHP:F0}";
        }
    }
    
    /// <summary>
    /// 체력 색상 업데이트
    /// </summary>
    private void UpdateHealthColor(float healthRatio)
    {
        Color targetColor;
        
        if (healthRatio <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else if (healthRatio <= mediumHealthThreshold)
        {
            targetColor = mediumHealthColor;
        }
        else
        {
            targetColor = fullHealthColor;
        }
        
        // 슬라이더 채우기 색상 변경
        if (healthFillImage != null)
        {
            healthFillImage.color = targetColor;
        }
        
        // 텍스트 색상도 변경
        if (healthText != null)
        {
            healthText.color = targetColor;
        }
    }
    
    /// <summary>
    /// 저체력 경고 처리
    /// </summary>
    private void HandleLowHealthWarning(float healthRatio)
    {
        bool shouldShowWarning = enableLowHealthWarning && healthRatio <= lowHealthThreshold && healthRatio > 0;
        
        if (shouldShowWarning && !isBlinking)
        {
            StartLowHealthWarning();
        }
        else if (!shouldShowWarning && isBlinking)
        {
            StopLowHealthWarning();
        }
    }
    
    /// <summary>
    /// 저체력 경고 시작
    /// </summary>
    private void StartLowHealthWarning()
    {
        isBlinking = true;
        if (lowHealthWarning != null)
        {
            lowHealthWarning.SetActive(true);
        }
        Debug.Log("Low Health Warning Started!");
    }
    
    /// <summary>
    /// 저체력 경고 중지
    /// </summary>
    private void StopLowHealthWarning()
    {
        isBlinking = false;
        if (lowHealthWarning != null)
        {
            lowHealthWarning.SetActive(false);
        }
    }
    
    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void OnPlayerDied()
    {
        StopLowHealthWarning();
        
        // 사망 UI 효과 (필요시 구현)
        Debug.Log("Player died - Health UI updated");
    }
    
    /// <summary>
    /// 수동으로 UI 업데이트
    /// </summary>
    [ContextMenu("Update Health Display")]
    public void ForceUpdateDisplay()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                UpdateHealthDisplay(playerHealth.CurrentHP, playerHealth.MaxHP);
            }
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
    /// 경고 설정 변경
    /// </summary>
    public void SetLowHealthWarningEnabled(bool enabled)
    {
        enableLowHealthWarning = enabled;
        if (!enabled)
        {
            StopLowHealthWarning();
        }
    }
}