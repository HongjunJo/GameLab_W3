using UnityEngine;

/// <summary>
/// 로켓 - 활성화 시 연결된 여러 오브젝트를 함께 활성화시키는 건물.
/// </summary>
public class Rocket : MonoBehaviour, IInteractable
{
    [Header("Rocket Settings")]
    [SerializeField] private BuildingRecipe activationRecipe;
    [SerializeField] private bool isActivated = false;

    [Header("Activation Target")]
    [Tooltip("로켓이 활성화될 때 함께 활성화될 오브젝트들의 배열입니다.")]
    [SerializeField] private GameObject[] objectsToActivate;

    [Header("Visual")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        // 시작 시 비활성화 상태로 초기화
        SetObjectsActive(false);
        UpdateVisual();

        // 상호작용을 위한 콜라이더가 없으면 추가
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            var newCol = gameObject.AddComponent<CircleCollider2D>();
            newCol.radius = 2f; // 적절한 반경 설정
            newCol.isTrigger = true;
        }
    }

    public bool CanInteract()
    {
        // 활성화되지 않았고, 비용이 충분할 때만 상호작용 가능
        return !isActivated && activationRecipe != null && activationRecipe.CanAfford();
    }

    public void Interact()
    {
        if (CanInteract())
        {
            ActivateRocket();
        }
    }

    public void StopInteract()
    {
        // 로켓은 홀드 상호작용이 없으므로 비워둡니다.
    }

    public string GetInteractionText()
    {
        if (isActivated)
        {
            return $"{activationRecipe.recipeName} (CLEAR!)";
        }

        string costText = activationRecipe.GetCostAsString();
        if (CanInteract())
        {
            return $"Prepare {activationRecipe.recipeName}\n{costText}";
        }
        else
        {
            return $"Prepare {activationRecipe.recipeName}\n<color=red>Need more resources</color>\n{costText}";
        }
    }

    private void ActivateRocket()
    {
        if (activationRecipe == null || !activationRecipe.CanAfford()) return;

        if (activationRecipe.ConsumeCost())
        {
            isActivated = true;

            // 연결된 오브젝트들 활성화
            SetObjectsActive(true);

            UpdateVisual();
            GameEvents.BuildingActivated($"Rocket_{activationRecipe.recipeName}");
            Debug.Log($"Rocket '{activationRecipe.recipeName}' has been activated!");
        }
    }

    private void SetObjectsActive(bool active)
    {
        if (objectsToActivate == null) return;

        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    private void UpdateVisual()
    {
        if (objectRenderer == null) return;

        objectRenderer.material = isActivated ? activeMaterial : inactiveMaterial;
    }
}