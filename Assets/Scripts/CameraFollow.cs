using UnityEngine;

[DisallowMultipleComponent]
public sealed class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Header("Follow")]
    [SerializeField, Min(0f)]
    private float followSpeed = 8f;

    private Vector3 offset;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError(
                "CameraFollow 初始化失敗，尚未設定追蹤目標。",
                this);
            enabled = false;
            return;
        }

        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime);
    }
}
