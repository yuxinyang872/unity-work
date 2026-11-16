using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovementAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform turnRoot;

    [Header("Turning")]
    [SerializeField, Min(0f)]
    private float turnSpeed = 720f;

    [Header("Animation")]
    [SerializeField, Min(0f)]
    private float movementThreshold = 0.05f;

    [SerializeField, Min(0f)]
    private float crossFadeDuration = 0.1f;

    [SerializeField]
    private string idleStateName = "Idle";

    [SerializeField]
    private string moveStateName = "Run";

    private CharacterController characterController;
    private int idleStateHash;
    private int moveStateHash;
    private int currentStateHash;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (turnRoot == null)
        {
            turnRoot = transform;
        }

        idleStateHash = Animator.StringToHash($"Base Layer.{idleStateName}");
        moveStateHash = Animator.StringToHash($"Base Layer.{moveStateName}");

        if (animator == null)
        {
            Debug.LogError("玩家動畫初始化失敗，找不到子物件上的 Animator。", this);
            enabled = false;
            return;
        }

        if (!animator.HasState(0, idleStateHash))
        {
            Debug.LogWarning($"Animator 找不到狀態 Base Layer.{idleStateName}。", animator);
        }

        if (!animator.HasState(0, moveStateHash))
        {
            Debug.LogWarning($"Animator 找不到狀態 Base Layer.{moveStateName}。", animator);
        }
    }

    private void LateUpdate()
    {
        Vector3 planarVelocity = characterController.velocity;
        planarVelocity.y = 0f;

        bool isMoving = planarVelocity.sqrMagnitude > movementThreshold * movementThreshold;

        if (isMoving)
        {
            RotateTowards(planarVelocity);
        }

        UpdateAnimation(isMoving);
    }

    private void RotateTowards(Vector3 movementDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        turnRoot.rotation = Quaternion.RotateTowards(
            turnRoot.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    private void UpdateAnimation(bool isMoving)
    {
        int targetStateHash = isMoving ? moveStateHash : idleStateHash;

        if (targetStateHash == currentStateHash || !animator.HasState(0, targetStateHash))
        {
            return;
        }

        animator.CrossFadeInFixedTime(targetStateHash, crossFadeDuration);
        currentStateHash = targetStateHash;
    }
}
