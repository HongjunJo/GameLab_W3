using UnityEngine;

public class MovementLimiter : MonoBehaviour
{
    public static MovementLimiter Instance;

    [SerializeField] public bool _initialCharacterCanMove = true;
    public bool CharacterCanMove;

    private void OnEnable()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CharacterCanMove = _initialCharacterCanMove;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 캐릭터의 이동 가능 상태를 설정합니다.
    /// </summary>
    /// <param name="canMove">이동 가능 여부</param>
    public void SetCanMove(bool canMove)
    {
        CharacterCanMove = canMove;
    }
}
