using UnityEngine;

/// <summary>
/// 지정된 축과 속도로 계속 회전하는 스크립트
/// </summary>
public class MovingBar : MonoBehaviour
{
    /// <summary>
    /// 회전할 축을 정의합니다.
    /// </summary>
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Rotation Settings")]
    [Tooltip("회전할 축을 선택합니다.")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

    [Tooltip("true로 설정하면 시작 시 회전 각도를 무작위로 설정하여 다른 막대와 겹치지 않게 합니다.")]
    [SerializeField] private bool randomizeStartRotation = true;

    [Tooltip("회전 속도입니다. 양수는 시계 방향, 음수는 반시계 방향입니다.")]
    [SerializeField] private float rotationSpeed = 100f;

    private Vector3 axisVector;

    private void Start()
    {
        // 선택된 축에 따라 회전 방향 벡터를 설정합니다.
        if (rotationAxis == RotationAxis.X) axisVector = Vector3.right;
        else if (rotationAxis == RotationAxis.Y) axisVector = Vector3.up;
        else axisVector = Vector3.forward; // Z축

        // 시작 각도 무작위 설정 옵션이 켜져 있으면, 시작 시 랜덤한 각도로 회전시킵니다.
        if (randomizeStartRotation)
        {
            transform.Rotate(axisVector, Random.Range(0f, 360f));
        }
    }

    private void Update()
    {
        // 매 프레임마다 지정된 축과 속도로 오브젝트를 회전시킵니다.
        transform.Rotate(axisVector, rotationSpeed * Time.deltaTime);
    }
}
