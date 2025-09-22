using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전반에 걸친 이벤트 시스템
/// 시스템들 간의 느슨한 결합을 위해 사용
/// </summary>
public static class GameEvents
{
    // 자원 관련 이벤트
    public static event Action<MineralData, int> OnResourceAdded;
    public static event Action<MineralData, int> OnResourceUsed;
    public static event Action<Dictionary<MineralData, int>> OnResourceChanged;
    
    // 전력 관련 이벤트
    public static event Action<float, float> OnPowerChanged; // currentPower, maxPower
    
    public static event Action OnPlayerDied;
    
    // 위험도 관련 이벤트
    public static event Action<float, float> OnDangerChanged; // currentDanger, maxDanger
    
    // 생산 관련 이벤트
    public static event Action<MineralData, int> OnMineralProduced;
    public static event Action<string> OnBuildingActivated; // building name
    
    // 안전지대 관련 이벤트
    public static event Action OnEnteredSafeZone;
    public static event Action OnExitedSafeZone;
    
    // 자원 이벤트 발생 메서드들
    public static void ResourceAdded(MineralData mineral, int amount)
    {
        OnResourceAdded?.Invoke(mineral, amount);
    }
    
    public static void ResourceUsed(MineralData mineral, int amount)
    {
        OnResourceUsed?.Invoke(mineral, amount);
    }
    
    public static void ResourceChanged(Dictionary<MineralData, int> resources)
    {
        OnResourceChanged?.Invoke(resources);
    }
    
    // 전력 이벤트 발생 메서드들
    public static void PowerChanged(float currentPower, float maxPower)
    {
        OnPowerChanged?.Invoke(currentPower, maxPower);
    }
    
    public static void PlayerDied()
    {
        OnPlayerDied?.Invoke();
    }
    
    // 위험도 이벤트 발생 메서드들
    public static void DangerChanged(float currentDanger, float maxDanger)
    {
        OnDangerChanged?.Invoke(currentDanger, maxDanger);
    }
    
    // 생산 이벤트 발생 메서드들
    public static void MineralProduced(MineralData mineral, int amount)
    {
        OnMineralProduced?.Invoke(mineral, amount);
    }
    
    public static void BuildingActivated(string buildingName)
    {
        OnBuildingActivated?.Invoke(buildingName);
    }
    
    // 안전지대 이벤트 발생 메서드들
    public static void EnteredSafeZone()
    {
        OnEnteredSafeZone?.Invoke();
    }
    
    public static void ExitedSafeZone()
    {
        OnExitedSafeZone?.Invoke();
    }
}