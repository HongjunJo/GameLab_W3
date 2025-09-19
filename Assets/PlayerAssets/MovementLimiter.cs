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
}
