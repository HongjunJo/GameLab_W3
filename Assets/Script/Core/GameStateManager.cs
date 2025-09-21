using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 상태 관리 매니저 - 플레이어 사망, 리스폰, 게임 진행 상태 관리
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    [SerializeField] private bool isPaused = false;
    
    [Header("Death Settings")]
    [SerializeField] private float deathScreenDuration = 3f;
    [SerializeField] private GameObject deathScreenUI;
    
    [Header("Statistics")]
    [SerializeField] private int deathCount = 0;
    [SerializeField] private float totalPlayTime = 0f;
    [SerializeField] private float currentSessionTime = 0f;
    
    private static GameStateManager instance;
    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameStateManager>();
            }
            return instance;
        }
    }
    
    public GameState CurrentState => currentState;
    public bool IsPaused => isPaused;
    public int DeathCount => deathCount;
    public float TotalPlayTime => totalPlayTime;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void OnEnable()
    {
        GameEvents.OnPlayerDied += OnPlayerDied;
    }
    
    private void OnDisable()
    {
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }
    
    private void Start()
    {
        SetGameState(GameState.Playing);
        
        // 사망 화면 UI 초기 비활성화
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (currentState == GameState.Playing && !isPaused)
        {
            currentSessionTime += Time.deltaTime;
            totalPlayTime += Time.deltaTime;
        }
    }
    
    /// <summary>
    /// 게임 상태 변경
    /// </summary>
    public void SetGameState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"Game state changed: {previousState} → {newState}");
        
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
                
            case GameState.Dead:
                // Time.timeScale = 0f; // 리스폰 코루틴이 멈추지 않도록 주석 처리
                // 게임은 계속 진행되지만, 플레이어는 제어 불능 상태가 됨
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
                
            case GameState.Loading:
                Time.timeScale = 0f;
                break;
        }
    }
    
    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void OnPlayerDied()
    {
        deathCount++;
        Debug.Log($"Player died! Total deaths: {deathCount}");
        
        SetGameState(GameState.Dead);
        StartCoroutine(HandleDeathSequence());
    }
    
    /// <summary>
    /// 사망 시퀀스 처리
    /// </summary>
    private IEnumerator HandleDeathSequence()
    {
        // 사망 화면 표시
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(true);
        }
        
        // 사망 화면 유지 시간
        yield return new WaitForSecondsRealtime(deathScreenDuration);
        
        // 사망 화면 숨기기
        if (deathScreenUI != null)
        {
            deathScreenUI.SetActive(false);
        }
        
        // 게임 재시작
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// 게임 일시정지/재개
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Dead) return;
        
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            isPaused = true;
            SetGameState(GameState.Paused);
            Debug.Log("Game paused");
        }
    }
    
    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            isPaused = false;
            SetGameState(GameState.Playing);
            Debug.Log("Game resumed");
        }
    }
    
    /// <summary>
    /// 게임 리셋 (새 게임)
    /// </summary>
    public void ResetGame()
    {
        deathCount = 0;
        currentSessionTime = 0f;
        
        // 플레이어 상태 리셋
        var dangerSystem = FindAnyObjectByType<DangerGaugeSystem>();
        if (dangerSystem != null)
        {
            dangerSystem.ResetDanger();
        }
        
        SetGameState(GameState.Playing);
        Debug.Log("Game reset");
    }
    
    /// <summary>
    /// 게임 통계 정보
    /// </summary>
    public string GetGameStats()
    {
        return $"Deaths: {deathCount}, Session Time: {currentSessionTime:F1}s, Total Time: {totalPlayTime:F1}s";
    }
    
    /// <summary>
    /// 현재 세션 시간 포맷팅
    /// </summary>
    public string GetFormattedSessionTime()
    {
        int minutes = Mathf.FloorToInt(currentSessionTime / 60f);
        int seconds = Mathf.FloorToInt(currentSessionTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    /// <summary>
    /// 죽음 없이 플레이한 시간 (세션 기준)
    /// </summary>
    public float GetSurvivalTime()
    {
        return currentSessionTime;
    }
}

/// <summary>
/// 게임 상태 열거형
/// </summary>
public enum GameState
{
    Loading,    // 로딩 중
    Playing,    // 플레이 중
    Dead,       // 플레이어 사망
    Paused,     // 일시정지
    GameOver    // 게임 오버
}