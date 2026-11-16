using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyUnit))]
public sealed class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool owner;
    private Rigidbody cachedRigidbody;
    private EnemyUnit enemyUnit;

    public EnemyPool Owner => owner;
    public bool IsAvailable { get; private set; } = true;

    private void Awake()
    {
        CacheComponents();
    }

    public void Initialize(EnemyPool pool)
    {
        owner = pool;
        IsAvailable = true;
        CacheComponents();
    }

    public bool Activate(Vector3 worldPosition)
    {
        CacheComponents();

        if (owner == null || !IsAvailable || enemyUnit == null || cachedRigidbody == null)
        {
            Debug.LogError("敵人無法從物件池啟用，必要組件或所屬物件池未設定。", this);
            return false;
        }

        transform.SetPositionAndRotation(worldPosition, Quaternion.identity);
        ResetPhysics();

        IsAvailable = false;
        gameObject.SetActive(true);
        enemyUnit.RestoreFullHealth();
        return true;
    }

    public void ReturnToPool()
    {
        if (owner == null)
        {
            Debug.LogWarning("敵人尚未設定所屬物件池。", this);
            return;
        }

        owner.Recycle(this);
    }

    internal void Deactivate()
    {
        ResetPhysics();
        IsAvailable = true;
        gameObject.SetActive(false);
    }

    private void CacheComponents()
    {
        if (cachedRigidbody == null)
        {
            cachedRigidbody = GetComponent<Rigidbody>();
        }

        if (enemyUnit == null)
        {
            enemyUnit = GetComponent<EnemyUnit>();
        }
    }

    private void ResetPhysics()
    {
        if (cachedRigidbody == null || cachedRigidbody.isKinematic)
        {
            return;
        }

        cachedRigidbody.linearVelocity = Vector3.zero;
        cachedRigidbody.angularVelocity = Vector3.zero;
    }
}
