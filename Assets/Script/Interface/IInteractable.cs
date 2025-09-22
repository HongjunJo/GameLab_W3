/// <summary>
/// 상호작용 가능한 오브젝트가 구현해야 하는 인터페이스
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 현재 상호작용이 가능한지 확인
    /// </summary>
    /// <returns>상호작용 가능 여부</returns>
    bool CanInteract();
    
    /// <summary>
    /// 상호작용 실행
    /// </summary>
    void Interact();
    
    /// <summary>
    /// 상호작용 정보 텍스트 반환 (선택사항)
    /// </summary>
    /// <returns>상호작용 설명 텍스트</returns>
    string GetInteractionText();

    /// <summary>
    /// 상호작용 중단 (홀드 상호작용용)
    /// </summary>
    void StopInteract();
}
