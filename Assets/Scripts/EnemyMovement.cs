using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyUnit))]
public sealed class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3f;

    [SerializeField, Min(0f)]
    private float stopDistance = 0.5f;

    [Header("Rotation")]
    [SerializeField]
    private bool rotateTowardTarget = true;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 10f;

    private Rigidbody enemyRigidbody;
    private EnemyUnit enemyUnit;
    private PlayerUnit playerUnit;

    public float MoveSpeed => moveSpeed;

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0f, value);
    }

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        enemyUnit = GetComponent<EnemyUnit>();

        if (enemyRigidbody == null)
        {
            Debug.LogError(
                "EnemyMovement 初始化失敗，找不到 Rigidbody。",
                this);

            enabled = false;
            return;
        }

        if (enemyUnit == null)
        {
            Debug.LogError(
                "EnemyMovement 初始化失敗，找不到 EnemyUnit。",
                this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            RegisterToGameManager();
        }
    }

    private void Start()
    {
        ResolvePlayer();
        RegisterToGameManager();
    }

    private void OnDisable()
    {
        UnregisterFromGameManager();
    }

    private void FixedUpdate()
    {
        if (playerUnit == null)
        {
            ResolvePlayer();

            if (playerUnit == null)
            {
                StopHorizontalMovement();
                return;
            }
        }

        if (playerUnit.IsDead)
        {
            StopHorizontalMovement();
            return;
        }

        MoveTowardPlayer();
    }

    private void ResolvePlayer()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "EnemyMovement 無法取得玩家，場景中不存在 GameManager。",
                this);

            playerUnit = null;
            return;
        }

        playerUnit = GameManager.Instance.GetPlayer();

        if (playerUnit != null)
        {
            return;
        }

        Debug.LogError(
            "EnemyMovement 無法取得玩家，GameManager 尚未設定 PlayerUnit。",
            this);
    }

    private void RegisterToGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "EnemyMovement 無法註冊 EnemyUnit，場景中不存在 GameManager。",
                this);

            return;
        }

        if (enemyUnit == null)
        {
            return;
        }

        GameManager.Instance.RegisterEnemy(enemyUnit);
    }

    private void UnregisterFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (enemyUnit == null)
        {
            return;
        }

        GameManager.Instance.UnregisterEnemy(enemyUnit);
    }

    private void MoveTowardPlayer()
    {
        Vector3 direction =
            playerUnit.Transform.position - transform.position;

        direction.y = 0f;

        float stopDistanceSqr =
            stopDistance * stopDistance;

        if (direction.sqrMagnitude <= stopDistanceSqr)
        {
            StopHorizontalMovement();
            return;
        }

        Vector3 normalizedDirection =
            direction.normalized;

        Vector3 currentVelocity =
            enemyRigidbody.linearVelocity;

        enemyRigidbody.linearVelocity = new Vector3(
            normalizedDirection.x * moveSpeed,
            currentVelocity.y,
            normalizedDirection.z * moveSpeed);

        if (rotateTowardTarget)
        {
            RotateTowardDirection(normalizedDirection);
        }
    }

    private void RotateTowardDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up);

        Quaternion newRotation =
            Quaternion.Slerp(
                enemyRigidbody.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime);

        enemyRigidbody.MoveRotation(newRotation);
    }

    private void StopHorizontalMovement()
    {
        Vector3 currentVelocity =
            enemyRigidbody.linearVelocity;

        enemyRigidbody.linearVelocity = new Vector3(
            0f,
            currentVelocity.y,
            0f);
    }
}