using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resourceText;

    // 각 인벤토리의 상태를 저장할 딕셔너리
    private Dictionary<MineralData, int> mainInventory = new Dictionary<MineralData, int>();
    private Dictionary<MineralData, (int amount, List<ResourceSource> sources)> tempInventory = new Dictionary<MineralData, (int, List<ResourceSource>)>();

    private void OnEnable()
    {
        // 메인 인벤토리와 임시 인벤토리의 변경 이벤트를 각각 구독
        GameEvents.OnResourceChanged += UpdateMainInventory;
        TemporaryInventory.OnTemporaryResourceChanged += UpdateTempInventory;
    }

    private void OnDisable()
    {
        GameEvents.OnResourceChanged -= UpdateMainInventory;
        TemporaryInventory.OnTemporaryResourceChanged -= UpdateTempInventory;
    }

    private void Start()
    {
        // 게임 시작 시 각 인벤토리의 현재 상태를 가져와서 초기화
        if (ResourceManager.Instance != null)
        {
            mainInventory = ResourceManager.Instance.GetAllResources();
        }

        // 플레이어의 TemporaryInventory 컴포넌트를 찾아 초기 상태를 가져옵니다.
        var playerTempInventory = FindFirstObjectByType<TemporaryInventory>();
        if (playerTempInventory != null)
        {
            tempInventory = playerTempInventory.GetAllTempResources();
        }

        // 초기 상태로 UI를 한 번 업데이트합니다.
        UpdateDisplay();
    }

    /// <summary>
    /// 메인 인벤토리 데이터가 변경될 때 호출됩니다.
    /// </summary>
    private void UpdateMainInventory(Dictionary<MineralData, int> resources)
    {
        mainInventory = resources;
        UpdateDisplay();
    }

    /// <summary>
    /// 임시 인벤토리 데이터가 변경될 때 호출됩니다.
    /// </summary>
    private void UpdateTempInventory(Dictionary<MineralData, (int amount, List<ResourceSource> sources)> resources)
    {
        tempInventory = resources;
        UpdateDisplay();
    }

    /// <summary>
    /// 저장된 인벤토리 정보들을 바탕으로 전체 UI 텍스트를 다시 그립니다.
    /// </summary>
    private void UpdateDisplay()
    {
        if (resourceText == null) return;

        StringBuilder displayTextBuilder = new StringBuilder();

        // 1. 메인 인벤토리 (저장된 자원)
        displayTextBuilder.AppendLine("<b>[창고]</b>");
        if (mainInventory.Count == 0)
        {
            displayTextBuilder.AppendLine("  (비어있음)");
        }
        else
        {
            foreach (var kvp in mainInventory)
            {
                if (kvp.Key != null && kvp.Value > 0) // 0개인 자원은 표시하지 않음
                {
                    displayTextBuilder.AppendLine($"  {kvp.Key.mineralName}: {kvp.Value}");
                }
            }
        }

        // 2. 임시 인벤토리 (현재 탐사에서 캔 광물)
        displayTextBuilder.AppendLine("\n<b>[인벤토리]</b> (사망 시 50% 손실)");
        if (tempInventory.Count == 0)
        {
            displayTextBuilder.AppendLine("  (비어있음)");
        }
        else
        {
            foreach (var kvp in tempInventory)
            {
                displayTextBuilder.AppendLine($"  {kvp.Key.mineralName}: {kvp.Value.amount}");
            }
        }

        resourceText.text = displayTextBuilder.ToString();
    }
}
