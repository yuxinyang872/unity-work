using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public sealed class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField, Min(0f)]
    private float damage = 10f;

    [SerializeField, Min(0.01f)]
    private float lifetime = 5f;

    [Header("Hit")]
    [SerializeField]
    private bool destroyOnHit = true;

    private Rigidbody projectileRigidbody;
    private float remainingLifetime;
    private bool isTerminated;
    private ProjectilePool projectilePool;

    public void BindPool(ProjectilePool owner)
    {
        projectilePool = owner;
    }

    private void Awake()
    {
        projectileRigidbody = GetComponent<Rigidbody>();

        if (projectileRigidbody == null)
        {
            Debug.LogError(
                "Projectile 初始化失敗，找不到 Rigidbody。",
                this);

            enabled = false;
        }
    }

    private void OnEnable()
    {
        remainingLifetime = lifetime;
        isTerminated = false;
    }

    private void Update()
    {
        if (isTerminated)
        {
            return;
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            Terminate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.collider);
    }

    private void TryHit(Collider targetCollider)
    {
        if (isTerminated)
        {
            return;
        }

        EnemyUnit enemyUnit =
            targetCollider.GetComponentInParent<EnemyUnit>();

        if (enemyUnit == null)
        {
            return;
        }

        if (enemyUnit.IsDead)
        {
            return;
        }

        float actualDamage =
            enemyUnit.TakeDamage(damage);

        if (actualDamage <= 0f)
        {
            return;
        }

        Debug.Log(
            $"投射物對 {enemyUnit.name} 造成 {actualDamage} 點傷害。",
            this);

        if (destroyOnHit)
        {
            Terminate();
        }
    }

    private void Terminate()
    {
        if (isTerminated)
        {
            return;
        }

        isTerminated = true;

        projectileRigidbody.linearVelocity = Vector3.zero;
        projectileRigidbody.angularVelocity = Vector3.zero;

        if (projectilePool != null)
        {
            projectilePool.Recycle(this);
            return;
        }

        Destroy(gameObject);
    }
}