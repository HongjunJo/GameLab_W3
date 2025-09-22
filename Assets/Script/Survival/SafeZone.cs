using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 안전지대의 범위를 정의하고 플레이어의 출입을 감지
/// Box Collider 2D를 사용하여 명확한 사각형 범위 설정
/// </summary>
public class SafeZone : MonoBehaviour
{
    [Header("Safe Zone Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Vector2 safeZoneSize = new Vector2(10f, 10f); // 사각형 크기
    
    [Header("Visual Settings")]
    [SerializeField] private Color safeZoneColor = Color.green;
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private bool showRuntimeVisual = true; // 게임 실행 중 범위 표시
    [SerializeField] private bool limitVisualToTopHalf = true; // Y축 이상으로만 비주얼 제한
    [SerializeField] private float visualMinimumY = 0f; // 표시할 최저 Y값 (이 값 이상으로만 표시)
    [SerializeField] private Material safeZoneMaterial; // 범위 표시용 머티리얼
    [SerializeField] private bool enableMerging = true; // SafeZone 합치기 활성화
    [SerializeField] private float mergeCheckInterval = 1f; // 합치기 체크 간격
    
    [Header("Effects")]
    [SerializeField] private GameObject enterEffect;
    [SerializeField] private GameObject exitEffect;
    
    private bool playerInSafeZone = false;
    private LineRenderer lineRenderer; // 런타임 범위 표시용
    private LineRenderer mergedLineRenderer; // 합쳐진 영역 표시용
    private List<SafeZone> overlappingSafeZones = new List<SafeZone>(); // 겹치는 SafeZone들
    private float lastMergeCheck = 0f;
    
    public bool IsActive 
    { 
        get => isActive; 
        set 
        { 
            isActive = value;
            GetComponent<Collider2D>().enabled = value;
        } 
    }
    
    public bool PlayerInSafeZone => playerInSafeZone;
    
    private void Update()
    {
        if (enableMerging && Time.time - lastMergeCheck > mergeCheckInterval)
        {
            CheckForOverlappingSafeZones();
            lastMergeCheck = Time.time;
        }
    }
    
    private void Awake()
    {
        // Collider2D 설정 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        
        // BoxCollider2D의 경우 크기 설정
        if (col is BoxCollider2D boxCol)
        {
            boxCol.size = safeZoneSize;
        }
        
        // 런타임 시각화 설정
        SetupRuntimeVisual();
    }
    
    private void SetupRuntimeVisual()
    {
        if (!showRuntimeVisual) return;
        
        // LineRenderer 컴포넌트 추가 또는 가져오기
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // LineRenderer 설정
        lineRenderer.material = safeZoneMaterial;
        lineRenderer.startColor = safeZoneColor;
        lineRenderer.endColor = safeZoneColor;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.loop = true; // 사각형을 닫기 위해
        lineRenderer.useWorldSpace = false; // 로컬 좌표 사용
        lineRenderer.positionCount = 4; // 사각형은 4개의 점
        
        // 기본 머티리얼이 없으면 스프라이트 기본 머티리얼 사용
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        // 합쳐진 영역용 LineRenderer 설정
        SetupMergedVisual();
        
        UpdateRuntimeVisual();
    }
    
    private void UpdateRuntimeVisual()
    {
        if (lineRenderer == null || !showRuntimeVisual) return;
        
        // 사각형 꼭짓점 계산
        float halfWidth = safeZoneSize.x * 0.5f;
        float halfHeight = safeZoneSize.y * 0.5f;
        
        Vector3[] positions;
        
        // Debug.Log($"SafeZone {gameObject.name}: limitVisualToTopHalf={limitVisualToTopHalf}, visualMinimumY={visualMinimumY}, transform.position.y={transform.position.y}");
        
        if (limitVisualToTopHalf)
        {
            // 설정된 최저 Y값 이상으로만 표시
            float safeZoneBottom = transform.position.y - halfHeight;
            float safeZoneTop = transform.position.y + halfHeight;
            
            // 최저 Y값을 월드 좌표에서 로컬 좌표로 변환
            float localMinY = visualMinimumY - transform.position.y;
            
            // 실제 표시할 범위 계산
            float displayBottom = Mathf.Max(-halfHeight, localMinY);
            float displayTop = halfHeight;
            
            // Debug.Log($"LocalMinY: {localMinY}, DisplayBottom: {displayBottom}, DisplayTop: {displayTop}");
            
            positions = new Vector3[4]
            {
                new Vector3(-halfWidth, displayBottom, 0), // 왼쪽 아래 (제한된)
                new Vector3(halfWidth, displayBottom, 0),  // 오른쪽 아래 (제한된)
                new Vector3(halfWidth, displayTop, 0),     // 오른쪽 위
                new Vector3(-halfWidth, displayTop, 0)     // 왼쪽 위
            };
        }
        else
        {
            Debug.Log("Using full rectangle display");
            // 전체 사각형 표시
            positions = new Vector3[4]
            {
                new Vector3(-halfWidth, -halfHeight, 0), // 왼쪽 아래
                new Vector3(halfWidth, -halfHeight, 0),  // 오른쪽 아래
                new Vector3(halfWidth, halfHeight, 0),   // 오른쪽 위
                new Vector3(-halfWidth, halfHeight, 0)   // 왼쪽 위
            };
        }
        
        lineRenderer.SetPositions(positions);
        
        // 활성화 상태에 따라 색상 변경 및 표시/숨김
        if (isActive)
        {
            Color visualColor = safeZoneColor;
            visualColor.a = 0.7f; // 약간 투명하게
            lineRenderer.startColor = visualColor;
            lineRenderer.endColor = visualColor;
            lineRenderer.enabled = showRuntimeVisual; // 활성화된 경우에만 표시
        }
        else
        {
            lineRenderer.enabled = false; // 비활성화된 경우 완전히 숨김
        }
    }
    
    private void SetupMergedVisual()
    {
        if (!enableMerging) return;
        
        // 합쳐진 영역용 LineRenderer 생성 (자식 오브젝트로)
        GameObject mergedVisualObj = new GameObject("MergedSafeZoneVisual");
        mergedVisualObj.transform.SetParent(transform);
        mergedVisualObj.transform.localPosition = Vector3.zero;
        
        mergedLineRenderer = mergedVisualObj.AddComponent<LineRenderer>();
        mergedLineRenderer.material = safeZoneMaterial;
        
        // 합쳐진 영역은 더 두껍고 다른 색상으로
        Color mergedColor = safeZoneColor;
        mergedColor.a = 0.3f; // 더 투명하게
        mergedLineRenderer.startColor = mergedColor;
        mergedLineRenderer.endColor = mergedColor;
        mergedLineRenderer.startWidth = 0.15f; // 더 두껍게
        mergedLineRenderer.endWidth = 0.15f;
        mergedLineRenderer.loop = true;
        mergedLineRenderer.useWorldSpace = true; // 월드 좌표 사용
        
        if (mergedLineRenderer.material == null)
        {
            mergedLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        mergedLineRenderer.enabled = false; // 기본적으로 비활성화
    }
    
    private void CheckForOverlappingSafeZones()
    {
        if (!enableMerging || !isActive) return;
        
        overlappingSafeZones.Clear();
        
        // 모든 SafeZone 찾기
        SafeZone[] allSafeZones = FindObjectsByType<SafeZone>(FindObjectsSortMode.None);
        
        foreach (SafeZone otherZone in allSafeZones)
        {
            if (otherZone == this || !otherZone.IsActive) continue;
            
            // 겹침 체크
            if (IsOverlapping(otherZone))
            {
                overlappingSafeZones.Add(otherZone);
            }
        }
        
        UpdateMergedVisual();
    }
    
    private bool IsOverlapping(SafeZone otherZone)
    {
        // 두 사각형이 겹치는지 체크
        Bounds thisBounds = GetBounds();
        Bounds otherBounds = otherZone.GetBounds();
        
        return thisBounds.Intersects(otherBounds);
    }
    
    private Bounds GetBounds()
    {
        Vector3 center = transform.position;
        Vector3 size = new Vector3(safeZoneSize.x, safeZoneSize.y, 0);
        return new Bounds(center, size);
    }
    
    private void UpdateMergedVisual()
    {
        if (mergedLineRenderer == null || !enableMerging) return;
        
        // 현재 SafeZone이 비활성화되어 있으면 모든 시각화 숨김
        if (!isActive)
        {
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            mergedLineRenderer.enabled = false;
            return;
        }
        
        // 활성화된 SafeZone들만 필터링
        List<SafeZone> activeSafeZones = new List<SafeZone>();
        foreach (SafeZone zone in overlappingSafeZones)
        {
            if (zone.IsActive)
            {
                activeSafeZones.Add(zone);
            }
        }
        
        if (activeSafeZones.Count > 0)
        {
            // 개별 SafeZone 라인 숨기기
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
            
            // 합쳐진 영역의 외곽선 계산 및 표시 (활성화된 것들만)
            List<Vector3> mergedOutline = CalculateMergedOutline(activeSafeZones);
            if (mergedOutline.Count > 0)
            {
                mergedLineRenderer.positionCount = mergedOutline.Count;
                mergedLineRenderer.SetPositions(mergedOutline.ToArray());
                mergedLineRenderer.enabled = showRuntimeVisual;
            }
        }
        else
        {
            // 겹치는 활성화된 SafeZone이 없으면 개별 라인 표시
            if (lineRenderer != null)
            {
                lineRenderer.enabled = showRuntimeVisual && isActive; // 활성화된 경우에만
            }
            mergedLineRenderer.enabled = false;
        }
    }
    
    private List<Vector3> CalculateMergedOutline(List<SafeZone> safeZonesToMerge = null)
    {
        List<Vector3> outline = new List<Vector3>();
        
        // 사용할 SafeZone 리스트 결정
        List<SafeZone> zonesToUse = safeZonesToMerge ?? overlappingSafeZones;
        
        // 간단한 구현: 모든 SafeZone의 경계를 포함하는 최대 사각형 계산
        float minX = transform.position.x - safeZoneSize.x * 0.5f;
        float maxX = transform.position.x + safeZoneSize.x * 0.5f;
        float minY = transform.position.y - safeZoneSize.y * 0.5f;
        float maxY = transform.position.y + safeZoneSize.y * 0.5f;
        
        foreach (SafeZone zone in zonesToUse)
        {
            Vector3 pos = zone.transform.position;
            Vector2 size = zone.safeZoneSize;
            
            minX = Mathf.Min(minX, pos.x - size.x * 0.5f);
            maxX = Mathf.Max(maxX, pos.x + size.x * 0.5f);
            minY = Mathf.Min(minY, pos.y - size.y * 0.5f);
            maxY = Mathf.Max(maxY, pos.y + size.y * 0.5f);
        }
        
        // 합쳐진 사각형의 꼭짓점들
        outline.Add(new Vector3(minX, minY, 0)); // 왼쪽 아래
        outline.Add(new Vector3(maxX, minY, 0)); // 오른쪽 아래
        outline.Add(new Vector3(maxX, maxY, 0)); // 오른쪽 위
        outline.Add(new Vector3(minX, maxY, 0)); // 왼쪽 위
        
        return outline;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            playerInSafeZone = true;
            
            // 입장 이펙트
            if (enterEffect != null)
            {
                Instantiate(enterEffect, other.transform.position, Quaternion.identity);
            }
            
            Debug.Log($"Player entered safe zone: {gameObject.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // 비활성화된 깃발에서는 안전지대 해제 처리를 하지 않음
        if (!isActive) return;
        
        if (other.CompareTag("Player"))
        {
            playerInSafeZone = false;
            
            // 퇴장 이펙트
            if (exitEffect != null)
            {
                Instantiate(exitEffect, other.transform.position, Quaternion.identity);
            }
            
            Debug.Log($"Player exited safe zone: {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 안전지대 활성화
    /// </summary>
    public void ActivateSafeZone()
    {
        IsActive = true;
        UpdateRuntimeVisual(); // 시각적 업데이트
        CheckForOverlappingSafeZones(); // 합쳐진 영역 업데이트
        Debug.Log($"Safe zone activated: {gameObject.name}");
    }
    
    /// <summary>
    /// 안전지대 비활성화
    /// </summary>
    public void DeactivateSafeZone()
    {
        IsActive = false;
        
        // 플레이어가 안에 있었다면 강제로 나간 것으로 처리
        if (playerInSafeZone)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                OnTriggerExit2D(player.GetComponent<Collider2D>());
            }
        }
        
        UpdateRuntimeVisual(); // 시각적 업데이트
        CheckForOverlappingSafeZones(); // 합쳐진 영역 업데이트
        Debug.Log($"Safe zone deactivated: {gameObject.name}");
    }
    
    /// <summary>
    /// 안전지대 크기 설정
    /// </summary>
    public void SetSize(Vector2 newSize)
    {
        safeZoneSize = newSize;
        
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
        {
            boxCol.size = safeZoneSize;
        }
        
        UpdateRuntimeVisual(); // 시각적 업데이트
        CheckForOverlappingSafeZones(); // 합쳐진 영역 업데이트
    }
    
    /// <summary>
    /// 런타임 시각적 표시 토글
    /// </summary>
    public void ToggleRuntimeVisual()
    {
        showRuntimeVisual = !showRuntimeVisual;
        UpdateRuntimeVisual();
    }
    
    /// <summary>
    /// 런타임 시각적 표시 설정
    /// </summary>
    public void SetRuntimeVisual(bool show)
    {
        showRuntimeVisual = show;
        UpdateRuntimeVisual();
    }
    
    /// <summary>
    /// Y축 제한 설정 (설정된 최저 Y값 이상으로만 표시)
    /// </summary>
    public void SetVisualTopHalfLimit(bool limit)
    {
        limitVisualToTopHalf = limit;
        UpdateRuntimeVisual();
    }
    
    /// <summary>
    /// 표시할 최저 Y값 설정
    /// </summary>
    public void SetVisualMinimumY(float minY)
    {
        visualMinimumY = minY;
        UpdateRuntimeVisual();
        Debug.Log($"SafeZone {gameObject.name}: Visual Minimum Y set to {minY}");
    }
    
    /// <summary>
    /// 강제로 비주얼 업데이트 (디버깅용)
    /// </summary>
    [ContextMenu("Force Update Visual")]
    public void ForceUpdateVisual()
    {
        Debug.Log($"Force updating visual for {gameObject.name}");
        UpdateRuntimeVisual();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        
        Gizmos.color = safeZoneColor;
        
        Vector3 center = transform.position;
        
        if (limitVisualToTopHalf)
        {
            // 설정된 최저 Y값 이상으로만 표시
            float halfWidth = safeZoneSize.x * 0.5f;
            float halfHeight = safeZoneSize.y * 0.5f;
            
            float safeZoneBottom = center.y - halfHeight;
            float safeZoneTop = center.y + halfHeight;
            
            // 실제 표시할 범위 계산
            float displayBottom = Mathf.Max(safeZoneBottom, visualMinimumY);
            float displayTop = safeZoneTop;
            
            Vector3 displayCenter = new Vector3(center.x, (displayBottom + displayTop) * 0.5f, center.z);
            Vector3 displaySize = new Vector3(safeZoneSize.x, displayTop - displayBottom, 0);
            
            // 와이어프레임 큐브 (제한된 범위만)
            Gizmos.DrawWireCube(displayCenter, displaySize);
            
            // 활성화 상태에 따라 색상 변경하여 채우기
            Color fillColor = safeZoneColor;
            fillColor.a = isActive ? 0.2f : 0.1f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(displayCenter, displaySize);
        }
        else
        {
            // 전체 사각형 표시
            Vector3 size = new Vector3(safeZoneSize.x, safeZoneSize.y, 0);
            
            // 와이어프레임 큐브
            Gizmos.DrawWireCube(center, size);
            
            // 활성화 상태에 따라 색상 변경하여 채우기
            Color fillColor = safeZoneColor;
            fillColor.a = isActive ? 0.2f : 0.1f;
            Gizmos.color = fillColor;
            Gizmos.DrawCube(center, size);
        }
    }
}