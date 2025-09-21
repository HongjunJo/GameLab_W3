using UnityEngine;

/// <summary>
/// 그라운드 위치 관리 및 카메라 Y 위치 자동 조정 시스템
/// </summary>
public class GroundManager : MonoBehaviour
{
    public static GroundManager Instance { get; private set; }
    
    [Header("Ground Settings")]
    [SerializeField] private float[] groundYPositions; // 그라운드 Y 위치 배열 (높은 순서대로 정렬)
    [SerializeField] private float cameraOffset = 6f; // 카메라 오프셋 (그라운드 + 6)
    
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera; // 메인 카메라 참조
    
    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 메인 카메라 자동 찾기
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 그라운드 배열을 높은 순서대로 정렬
        SortGroundPositions();
        
        // 게임 시작 시 플레이어 위치를 기준으로 카메라 설정
        InitializeCameraForPlayer();
        
        Debug.Log($"GroundManager 초기화 완료. 총 {groundYPositions.Length}개의 그라운드 등록됨");
    }
    
    /// <summary>
    /// 게임 시작 시 플레이어 위치 기준으로 카메라 초기화
    /// </summary>
    private void InitializeCameraForPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float playerY = player.transform.position.y;
            AdjustCameraForPlayerPosition(playerY);
            Debug.Log($"게임 시작: 플레이어 위치({playerY}) 기준으로 카메라 설정");
        }
        else
        {
            // 플레이어를 찾을 수 없다면 0번 인덱스 사용
            SetCameraToGround(0);
            Debug.LogWarning("플레이어를 찾을 수 없어 0번 그라운드로 카메라 설정");
        }
    }
    
    /// <summary>
    /// 그라운드 배열을 높은 순서대로 정렬
    /// </summary>
    private void SortGroundPositions()
    {
        if (groundYPositions != null && groundYPositions.Length > 0)
        {
            System.Array.Sort(groundYPositions, (a, b) => b.CompareTo(a)); // 내림차순 정렬
            Debug.Log("그라운드 위치 정렬 완료 (높은 순서대로)");
        }
    }
    
    /// <summary>
    /// 플레이어 Y 좌표에 맞는 그라운드 찾기 및 카메라 위치 조정
    /// </summary>
    public void AdjustCameraForPlayerPosition(float playerY)
    {
        if (groundYPositions == null || groundYPositions.Length == 0)
        {
            Debug.LogWarning("그라운드 위치가 등록되지 않았습니다!");
            return;
        }
        
        // 높은 순서부터 검사하여 플레이어 Y보다 낮은 첫 번째 그라운드 찾기
        for (int i = 0; i < groundYPositions.Length; i++)
        {
            if (playerY >= groundYPositions[i])
            {
                SetCameraToGround(i);
                Debug.Log($"플레이어 Y: {playerY}, 선택된 그라운드: {groundYPositions[i]}, 카메라 Y: {groundYPositions[i] + cameraOffset}");
                return;
            }
        }
        
        // 모든 그라운드보다 낮은 위치라면 가장 낮은 그라운드 사용
        int lastIndex = groundYPositions.Length - 1;
        SetCameraToGround(lastIndex);
        Debug.Log($"플레이어가 모든 그라운드보다 낮음. 가장 낮은 그라운드 사용: {groundYPositions[lastIndex]}");
    }
    
    /// <summary>
    /// 특정 인덱스의 그라운드에 맞춰 카메라 Y 위치 설정
    /// </summary>
    private void SetCameraToGround(int groundIndex)
    {
        if (groundIndex < 0 || groundIndex >= groundYPositions.Length)
        {
            Debug.LogError($"잘못된 그라운드 인덱스: {groundIndex}");
            return;
        }
        
        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            cameraPos.y = groundYPositions[groundIndex] + cameraOffset;
            mainCamera.transform.position = cameraPos;
            
            Debug.Log($"카메라 Y 위치 조정: {cameraPos.y} (그라운드: {groundYPositions[groundIndex]} + 오프셋: {cameraOffset})");
        }
        else
        {
            Debug.LogError("메인 카메라 참조가 없습니다!");
        }
    }
    
    /// <summary>
    /// 그라운드 위치 배열 동적 추가
    /// </summary>
    public void AddGroundPosition(float groundY)
    {
        if (groundYPositions == null)
        {
            groundYPositions = new float[] { groundY };
        }
        else
        {
            // 기존 배열 크기 확장
            float[] newArray = new float[groundYPositions.Length + 1];
            System.Array.Copy(groundYPositions, newArray, groundYPositions.Length);
            newArray[groundYPositions.Length] = groundY;
            groundYPositions = newArray;
        }
        
        // 다시 정렬
        SortGroundPositions();
        Debug.Log($"새로운 그라운드 위치 추가: {groundY}");
    }
    
    /// <summary>
    /// 현재 등록된 그라운드 위치 출력 (디버깅용)
    /// </summary>
    [ContextMenu("Print Ground Positions")]
    public void PrintGroundPositions()
    {
        if (groundYPositions == null || groundYPositions.Length == 0)
        {
            Debug.Log("등록된 그라운드가 없습니다.");
            return;
        }
        
        Debug.Log("=== 등록된 그라운드 위치 ===");
        for (int i = 0; i < groundYPositions.Length; i++)
        {
            Debug.Log($"인덱스 {i}: Y = {groundYPositions[i]}");
        }
    }
    
    /// <summary>
    /// 카메라 오프셋 변경
    /// </summary>
    public void SetCameraOffset(float newOffset)
    {
        cameraOffset = newOffset;
        Debug.Log($"카메라 오프셋 변경: {cameraOffset}");
    }
    
    /// <summary>
    /// 현재 카메라가 위치한 그라운드 인덱스 반환
    /// </summary>
    public int GetCurrentGroundIndex()
    {
        if (mainCamera == null || groundYPositions == null) return -1;
        
        float currentCameraY = mainCamera.transform.position.y;
        
        for (int i = 0; i < groundYPositions.Length; i++)
        {
            if (Mathf.Approximately(currentCameraY, groundYPositions[i] + cameraOffset))
            {
                return i;
            }
        }
        
        return -1; // 해당하는 그라운드 없음
    }
}