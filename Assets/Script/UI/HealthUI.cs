using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;
    
    private void OnEnable()
    {
        GameEvents.OnHealthChanged += UpdateDisplay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnHealthChanged -= UpdateDisplay;
    }
    
    private void Start()
    {
        if (healthText != null)
        {
            healthText.text = "HP: 100/100";
        }
        
        if (healthSlider != null)
        {
            healthSlider.value = 1f;
        }
    }
    
    private void UpdateDisplay(float current, float maximum)
    {
        // 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"HP: {current:F0}/{maximum:F0}";
        }
        
        // 슬라이더 업데이트
        if (healthSlider != null)
        {
            healthSlider.value = maximum > 0 ? current / maximum : 0;
        }
        
        // 체력 상태에 따른 색상 변경
        float healthRatio = maximum > 0 ? current / maximum : 0;
        Color healthColor = Color.green;
        
        if (healthRatio <= 0.3f)
        {
            healthColor = Color.red;
        }
        else if (healthRatio <= 0.6f)
        {
            healthColor = Color.yellow;
        }
        
        // 텍스트 색상 변경
        if (healthText != null)
        {
            healthText.color = healthColor;
        }
        
        // 슬라이더 Fill 색상 변경
        if (healthFill != null)
        {
            healthFill.color = healthColor;
        }
    }
}
