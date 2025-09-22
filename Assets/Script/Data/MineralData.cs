using UnityEngine;

[CreateAssetMenu(fileName = "New Mineral", menuName = "Game Data/Mineral Data")]
public class MineralData : ScriptableObject
{
    [Header("Basic Info")]
    public string mineralName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Properties")]
    public Color mineralColor = Color.white;

    [Header("Harvesting Properties")]
    public int maxHp = 3; // 채집에 필요한 타격 횟수 (내구도)
    public int miningDamage = 1; // 상호작용 시 광물이 입는 피해량 (기본 1)
    public float miningDuration = 2f; // 채집에 필요한 홀드 시간 (초)
    public float respawnTime = 10f; // 채집 후 재생성까지 걸리는 시간 (초)
}