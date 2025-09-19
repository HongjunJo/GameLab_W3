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
        // 초기화 시 None으로 설정
        if (resourceText != null)
        {
            resourceText.text = "Resources: None";
        }
        
        // ResourceManager가 이미 자원을 가지고 있다면 즉시 업데이트
        if (ResourceManager.Instance != null)
        {
            UpdateDisplay(ResourceManager.Instance.GetAllResources());
        }
    }
    
    private void UpdateDisplay(Dictionary<MineralData, int> resources)
    {
        if (resourceText == null) return;
        
        string displayText = "Resources:\n";
        
        foreach (var kvp in resources)
        {
            if (kvp.Key != null)
            {
                // 한 번이라도 습득한 자원은 0이어도 표시
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
