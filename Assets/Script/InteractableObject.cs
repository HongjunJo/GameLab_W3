using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private string interactionMessage = "Interact";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool destroyOnInteract = true;
    
    [Header("Effects")]
    [SerializeField] private GameObject interactionEffect;
    [SerializeField] private AudioClip interactionSound;
    
    public bool CanInteract()
    {
        return canInteract;
    }
    
    public void Interact()
    {
        if (!CanInteract()) return;
        
        // 상호작용 이펙트 생성
        if (interactionEffect != null)
        {
            Instantiate(interactionEffect, transform.position, Quaternion.identity);
        }
        
        // 상호작용 사운드 재생
        if (interactionSound != null)
        {
            AudioSource.PlayClipAtPoint(interactionSound, transform.position);
        }
        
        // 상호작용 시 콘솔에 메시지를 출력
        Debug.Log($"Interaction successful with {gameObject.name}: {interactionMessage}");
        
        // 설정에 따라 오브젝트 파괴
        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
        else
        {
            // 한 번 상호작용하면 비활성화
            canInteract = false;
        }
    }
    
    public string GetInteractionText()
    {
        return canInteract ? interactionMessage : "Cannot interact";
    }
    
    /// <summary>
    /// 상호작용 활성화/비활성화
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        canInteract = interactable;
    }
    
    /// <summary>
    /// 상호작용 메시지 변경
    /// </summary>
    public void SetInteractionMessage(string message)
    {
        interactionMessage = message;
    }
}