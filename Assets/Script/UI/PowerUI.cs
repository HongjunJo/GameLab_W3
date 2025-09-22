using UnityEngine;
using TMPro;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI powerText;
    
    private void OnEnable()
    {
        GameEvents.OnPowerChanged += UpdateDisplay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPowerChanged -= UpdateDisplay;
    }
    
    private void Start()
    {
        if (powerText != null)
        {
            powerText.text = "Power: 0";
        }
    }
    
    private void UpdateDisplay(float current, float maximum)
    {
        if (powerText == null) return;
        
        powerText.text = $"Power: {current:F0}";
        
        // 전력 상태에 따른 색상 변경
        float powerRatio = maximum > 0 ? current / maximum : 0;
        
        if (powerRatio >= 0.7f)
        {
            powerText.color = Color.green;
        }
        else if (powerRatio >= 0.3f)
        {
            powerText.color = Color.yellow;
        }
        else
        {
            powerText.color = Color.red;
        }
    }
}
