using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceText;
    
    private void OnEnable()
    {
        GameEvents.OnResourceChanged += UpdateDisplay;
    }
    
    private void OnDisable()
    {
        GameEvents.OnResourceChanged -= UpdateDisplay;
    }
    
    private void Start()
    {
        // 초기화 시 빈 텍스트로 설정
        if (resourceText != null)
        {
            resourceText.text = "Resources: Loading...";
        }
    }
    
    private void UpdateDisplay(Dictionary<MineralData, int> resources)
    {
        if (resourceText == null) return;
        
        string displayText = "Resources:\n";
        
        foreach (var kvp in resources)
        {
            if (kvp.Key != null && kvp.Value > 0)
            {
                displayText += $"{kvp.Key.mineralName}: {kvp.Value}\n";
            }
        }
        
        // 자원이 없는 경우
        if (displayText == "Resources:\n")
        {
            displayText = "Resources: None";
        }
        
        resourceText.text = displayText;
    }
}
