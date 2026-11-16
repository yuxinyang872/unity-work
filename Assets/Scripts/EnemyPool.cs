using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyPool : MonoBehaviour
{
    [Header("敵人 Prefab")]
    [SerializeField] private EnemyPoolMember enemyPrefab;

    [Header("物件池容量")]
    [SerializeField, Min(0)] private int initialCapacity = 20;
    [SerializeField, Min(1)] private int maxCapacity = 40;

    private readonly Queue<EnemyPoolMember> availableEnemies = new();
    private Transform creationRoot;
    private int createdCount;

    public int CreatedCount => createdCount;
    public int AvailableCount => availableEnemies.Count;
    public int ActiveCount => createdCount - availableEnemies.Count;

    private void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("敵人物件池未指定敵人 Prefab。", this);
            enabled = false;
            return;
        }

        maxCapacity = Mathf.Max(1, maxCapacity);
        initialCapacity = Mathf.Clamp(initialCapacity, 0, maxCapacity);

        creationRoot = new GameObject("EnemyCreationRoot").transform;
        creationRoot.SetParent(transform, false);
        creationRoot.gameObject.SetActive(false);

        for (int i = 0; i < initialCapacity; i++)
        {
            availableEnemies.Enqueue(CreateEnemy());
        }
    }

    public EnemyPoolMember Spawn(Vector3 worldPosition)
    {
        if (enemyPrefab == null || creationRoot == null)
        {
            return null;
        }

        EnemyPoolMember enemy;

        if (availableEnemies.Count > 0)
        {
            enemy = availableEnemies.Dequeue();
        }
        else if (createdCount < maxCapacity)
        {
            enemy = CreateEnemy();
        }
        else
        {
            return null;
        }

        if (!enemy.Activate(worldPosition))
        {
            availableEnemies.Enqueue(enemy);
            return null;
        }

        return enemy;
    }

    public void Recycle(EnemyPoolMember enemy)
    {
        if (enemy == null || enemy.Owner != this || enemy.IsAvailable)
        {
            return;
        }

        enemy.Deactivate();
        availableEnemies.Enqueue(enemy);
    }

    private EnemyPoolMember CreateEnemy()
    {
        EnemyPoolMember enemy = Instantiate(enemyPrefab, creationRoot);
        enemy.name = $"Enemy_{createdCount:00}";
        enemy.Initialize(this);
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(transform, true);

        createdCount++;
        return enemy;
    }
}
