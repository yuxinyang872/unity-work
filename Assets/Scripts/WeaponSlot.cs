using UnityEngine;

public sealed class WeaponSlot : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField]
    private Projectile projectileTemplate;

    [SerializeField]
    private Transform firePoint;

    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 10f;

    [Header("Targeting")]
    [SerializeField, Min(0f)]
    private float attackRange = 10f;

    [SerializeField]
    private string enemyTag = "Enemy";

    [Header("Attack")]
    [SerializeField, Min(0.01f)]
    private float attackInterval = 0.5f;

    private float nextAttackTime;
    private ProjectilePool projectilePool;

    private void Awake()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
        }
    }

    private void Start()
    {
        if (!enabled)
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("WeaponSlot 找不到 GameManager。", this);
            enabled = false;
            return;
        }

        projectilePool = gameManager.GetProjectilePool();

        if (projectilePool == null)
        {
            Debug.LogError("WeaponSlot 無法取得投射物池，請設定 GameManager 的 ProjectilePool。", this);
            enabled = false;
            return;
        }

        if (!projectilePool.Prepare(projectileTemplate))
        {
            Debug.LogError("WeaponSlot 無法準備投射物池。", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        Transform target = FindNearestTarget();

        if (target == null)
        {
            return;
        }

        Fire(target);
        nextAttackTime = Time.time + attackInterval;
    }

    private Transform FindNearestTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange);
        Transform nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        foreach (Collider targetCollider in colliders)
        {
            if (!targetCollider.CompareTag(enemyTag))
            {
                continue;
            }

            Vector3 offset = targetCollider.transform.position - transform.position;
            float distanceSqr = offset.sqrMagnitude;

            if (distanceSqr >= nearestDistanceSqr)
            {
                continue;
            }

            nearestDistanceSqr = distanceSqr;
            nearestTarget = targetCollider.transform;
        }

        return nearestTarget;
    }

    private void Fire(Transform target)
    {
        if (projectilePool == null || projectileTemplate == null)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        Vector3 direction = target.position - spawnPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        direction.Normalize();
        Quaternion spawnRotation = Quaternion.LookRotation(direction, Vector3.up);
        Projectile projectile = projectilePool.Spawn(projectileTemplate, spawnPosition, spawnRotation);

        if (projectile == null)
        {
            return;
        }

        if (!projectile.TryGetComponent(out Rigidbody projectileRigidbody))
        {
            Debug.LogError("投射物缺少 Rigidbody。", projectile);
            projectilePool.Recycle(projectile);
            return;
        }

        projectileRigidbody.linearVelocity = direction * projectileSpeed;
    }

    private Vector3 GetSpawnPosition()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return projectileTemplate.transform.position;
    }

    private bool ValidateConfiguration()
    {
        if (projectileTemplate == null)
        {
            Debug.LogError("WeaponSlot 尚未設定 Projectile Template。", this);
            return false;
        }

        if (!projectileTemplate.transform.IsChildOf(transform))
        {
            Debug.LogWarning("Projectile Template 建議掛載在 WeaponSlot 物件底下。", this);
        }

        projectileTemplate.gameObject.SetActive(false);

        if (!projectileTemplate.TryGetComponent(out Rigidbody projectileRigidbody))
        {
            Debug.LogError("Projectile Template 缺少 Rigidbody。", projectileTemplate);
            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
