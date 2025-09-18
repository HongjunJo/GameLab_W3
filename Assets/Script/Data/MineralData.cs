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
    public bool isSpecialMineral; // 특정 행성에서만 나오는 광물인지
    public int baseValue = 1; // 기본 가치 (나중에 거래 시스템에서 사용)
}