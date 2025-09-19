using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject interactionPanel;
    
    private static InteractionUI instance;
    
    private void Awake()
    {
        // 간단한 싱글톤 패턴
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        HideInteraction();
    }
    
    public static void ShowMessage(string message)
    {
        if (instance != null)
        {
            instance.ShowInteraction(message);
        }
    }
    
    public static void HideMessage()
    {
        if (instance != null)
        {
            instance.HideInteraction();
        }
    }
    
    private void ShowInteraction(string message)
    {
        if (interactionText != null)
        {
            interactionText.text = message;
        }
        
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(true);
        }
    }
    
    private void HideInteraction()
    {
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
    }
}
