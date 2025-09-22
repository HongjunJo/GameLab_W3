using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 플레이어의 임시 인벤토리를 관리합니다.
/// 탐험 지역에서 획득한 자원은 여기에 보관되며, 메인 섹터로 돌아가야 ResourceManager에 등록됩니다.
/// </summary>
public class TemporaryInventory : MonoBehaviour
{
    // MineralData를 키로, 획득한 양과 원래 ResourceSource를 값으로 저장
    // C# 7.0 ValueTuple을 사용합니다. (amount, sources)
    private readonly Dictionary<MineralData, (int amount, List<ResourceSource> sources)> tempResources = new Dictionary<MineralData, (int, List<ResourceSource>)>();
    public static event Action<Dictionary<MineralData, (int amount, List<ResourceSource> sources)>> OnTemporaryResourceChanged;

    [System.Serializable]
    public class TempResourceDisplay
    {
        public MineralData mineral;
        public int amount;
    }

    [Header("Debug - Temporary Resources")]
    [SerializeField] private List<TempResourceDisplay> debugTempResources = new List<TempResourceDisplay>();

    /// <summary>
    /// 임시 인벤토리에 자원을 추가합니다.
    /// </summary>
    /// <param name="mineral">추가할 광물 데이터</param>
    /// <param name="amount">추가할 양</param>
    /// <param name="source">자원의 출처 (재생성을 위해)</param>
    public void AddResource(MineralData mineral, int amount, ResourceSource source)
    {
        if (tempResources.ContainsKey(mineral))
        {
            var entry = tempResources[mineral];
            entry.amount += amount;
            entry.sources.Add(source);
            tempResources[mineral] = entry;
        }
        else
        {
            tempResources[mineral] = (amount, new List<ResourceSource> { source });
        }

        Debug.Log($"[임시] {mineral.mineralName} {amount}개 추가. 현재 임시 보유량: {tempResources[mineral].amount}");
        UpdateDebugDisplay();

        // 임시 인벤토리 변경 사항을 UI에 알립니다.
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
    }

    /// <summary>
    /// 임시 인벤토리의 모든 자원을 ResourceManager로 옮기고 임시 인벤토리를 비웁니다.
    /// </summary>
    public void DepositAllToMainInventory()
    {
        foreach (var item in tempResources)
        {
            ResourceManager.Instance.AddResource(item.Key, item.Value.amount);
        }
        Debug.Log($"임시 인벤토리의 모든 자원을 메인 인벤토리로 이전했습니다.");
        tempResources.Clear();

        // 임시 인벤토리가 비워졌음을 UI에 알립니다.
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
        UpdateDebugDisplay();
    }

    /// <summary>
    /// 임시 인벤토리를 비우고, 각 자원의 출처(ResourceSource)에 리스폰을 요청합니다.
    /// </summary>
    public void ClearAndRespawnAll()
    {
        Debug.Log("플레이어 사망. 임시 인벤토리의 자원 중 50%를 잃고, 각 자원을 리스폰시킵니다.");

        // 임시 인벤토리의 키 목록을 복사하여 반복 중 수정이 가능하도록 합니다.
        var mineralsToProcess = new List<MineralData>(tempResources.Keys);

        foreach (var mineral in mineralsToProcess)
        {
            var entry = tempResources[mineral];
            int currentAmount = entry.amount;
            
            // 남는 양을 50% 내림으로 계산합니다.
            int amountToKeep = Mathf.FloorToInt(currentAmount * 0.5f);
            int amountToLose = currentAmount - amountToKeep;
            Debug.Log($"{mineral.mineralName} {currentAmount}개 중 {amountToLose}개를 잃고 {amountToKeep}개가 남습니다.");


            // 잃을 양만큼 자원 소스를 찾아 리스폰시킵니다.
            for (int i = 0; i < amountToLose; i++)
            {
                if (entry.sources.Count > 0)
                {
                    entry.sources[0].ForceRespawn(); // 가장 먼저 추가된 소스부터 리스폰
                    entry.sources.RemoveAt(0);
                }
            }

            // 남은 양으로 인벤토리 업데이트
            if (amountToKeep > 0)
            {
                entry.amount = amountToKeep;
                tempResources[mineral] = entry;
            }
            else
            {
                tempResources.Remove(mineral); // 수량이 0이 되면 딕셔너리에서 완전히 제거
            }
        }

        // 임시 인벤토리가 비워졌음을 UI에 알립니다.
        OnTemporaryResourceChanged?.Invoke(new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources));
        UpdateDebugDisplay();
    }

    /// <summary>
    /// 현재 임시 자원 목록을 반환합니다.
    /// </summary>
    public Dictionary<MineralData, (int amount, List<ResourceSource> sources)> GetAllTempResources()
    {
        return new Dictionary<MineralData, (int, List<ResourceSource>)>(tempResources);
    }

    private void UpdateDebugDisplay()
    {
        debugTempResources.Clear();
        foreach (var kvp in tempResources)
        {
            debugTempResources.Add(new TempResourceDisplay { mineral = kvp.Key, amount = kvp.Value.amount });
        }
    }
}