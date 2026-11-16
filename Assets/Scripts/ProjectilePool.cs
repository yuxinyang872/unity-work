using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ProjectilePool : MonoBehaviour
{
    [Header("每種投射物的容量")]
    [SerializeField, Min(0), Tooltip("每種 Projectile 模板預先建立的數量。")]
    private int initialCapacity = 30;

    [SerializeField, Min(1), Tooltip("每種 Projectile 模板最多建立的數量。")]
    private int maxCapacity = 80;

    private sealed class TemplatePool
    {
        public readonly Queue<Projectile> Available = new();
        public readonly HashSet<Projectile> Active = new();
        public int CreatedCount;
    }

    private readonly Dictionary<Projectile, TemplatePool> templatePools = new();
    private readonly Dictionary<Projectile, TemplatePool> projectileOwners = new();

    private Transform poolRoot;
    private Transform creationRoot;

    public int CreatedCount { get; private set; }
    public int ActiveCount { get; private set; }
    public int AvailableCount => CreatedCount - ActiveCount;

    private void Awake()
    {
        maxCapacity = Mathf.Max(1, maxCapacity);
        initialCapacity = Mathf.Clamp(initialCapacity, 0, maxCapacity);

        poolRoot = new GameObject($"{name}_ProjectilePoolRoot").transform;
        creationRoot = new GameObject($"{name}_CreationRoot").transform;
        creationRoot.SetParent(poolRoot);
        creationRoot.gameObject.SetActive(false);
    }

    public bool Prepare(Projectile template)
    {
        if (template == null)
        {
            Debug.LogError("無法準備投射物池，缺少 Projectile Template。", this);
            return false;
        }

        if (templatePools.ContainsKey(template))
        {
            return true;
        }

        TemplatePool templatePool = new();
        templatePools.Add(template, templatePool);

        for (int i = 0; i < initialCapacity; i++)
        {
            templatePool.Available.Enqueue(CreateProjectile(template, templatePool));
        }

        return true;
    }

    public Projectile Spawn(Projectile template, Vector3 position, Quaternion rotation)
    {
        if (!Prepare(template))
        {
            return null;
        }

        TemplatePool templatePool = templatePools[template];
        Projectile projectile;

        if (templatePool.Available.Count > 0)
        {
            projectile = templatePool.Available.Dequeue();
        }
        else if (templatePool.CreatedCount < maxCapacity)
        {
            projectile = CreateProjectile(template, templatePool);
        }
        else
        {
            return null;
        }

        projectile.transform.SetPositionAndRotation(position, rotation);
        ResetVelocity(projectile);

        templatePool.Active.Add(projectile);
        ActiveCount++;
        projectile.gameObject.SetActive(true);

        return projectile;
    }

    public void Recycle(Projectile projectile)
    {
        if (projectile == null || !projectileOwners.TryGetValue(projectile, out TemplatePool templatePool))
        {
            return;
        }

        if (!templatePool.Active.Remove(projectile))
        {
            return;
        }

        ActiveCount--;
        ResetVelocity(projectile);
        projectile.gameObject.SetActive(false);
        templatePool.Available.Enqueue(projectile);
    }

    private Projectile CreateProjectile(Projectile template, TemplatePool templatePool)
    {
        Projectile projectile = Instantiate(template, creationRoot);
        projectile.name = $"Projectile_{CreatedCount:00}";
        projectile.BindPool(this);
        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(poolRoot, false);

        projectileOwners.Add(projectile, templatePool);
        templatePool.CreatedCount++;
        CreatedCount++;
        return projectile;
    }

    private static void ResetVelocity(Projectile projectile)
    {
        if (!projectile.TryGetComponent(out Rigidbody body) || body.isKinematic)
        {
            return;
        }

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    private void OnDestroy()
    {
        if (poolRoot != null)
        {
            Destroy(poolRoot.gameObject);
        }
    }
}
