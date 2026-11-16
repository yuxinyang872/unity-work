using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [Header("場景參照")]
    [SerializeField, Tooltip("負責提供目前可見地板範圍的控制器。")]
    private InfiniteGroundController groundController;

    [Header("敵人生成")]
    [SerializeField, Min(0.05f), Tooltip("每隔幾秒執行一次生成。")]
    private float spawnInterval = 0.75f;

    [SerializeField, Min(1), Tooltip("每次生成的敵人數量。")]
    private int enemiesPerSpawn = 2;

    [SerializeField, Min(1), Tooltip("場上允許同時存在的敵人數量上限。")]
    private int maxActiveEnemies = 30;

    [SerializeField, Min(0.1f), Tooltip("以玩家為中心的方形生成區域半邊長。")]
    private float spawnRadius = 7f;

    [SerializeField, Min(0f), Tooltip("生成位置與地板外緣保持的最小距離。")]
    private float groundEdgePadding = 1f;

    [SerializeField, Tooltip("敵人生成位置相對於地板的高度。")]
    private float spawnHeight = 0.5f;

    [Header("敵人回收")]
    [SerializeField, Min(0f), Tooltip("敵人超出目前地板區域多少距離後回收。")]
    private float recycleMargin = 5f;

    [Header("執行資訊")]
    [SerializeField, Tooltip("本次執行累計成功生成的敵人數量。")]
    private int totalSpawned;

    private readonly List<EnemyPoolMember> activeEnemies = new();
    private EnemyPool enemyPool;
    private PlayerUnit player;
    private float spawnTimer;
    private bool capacityWarningLogged;

    public int ActiveEnemyCount => activeEnemies.Count;
    public int TotalSpawned => totalSpawned;

    private void Start()
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("敵人生成器初始化失敗，場景中找不到 GameManager。", this);
            enabled = false;
            return;
        }

        enemyPool = gameManager.GetEnemyPool();
        player = gameManager.GetPlayer();

        if (enemyPool == null || player == null || groundController == null)
        {
            Debug.LogError("敵人生成器初始化失敗，請確認 GameManager 的玩家、敵人池與地板控制器。", this);
            enabled = false;
            return;
        }

        spawnInterval = Mathf.Max(0.05f, spawnInterval);
        enemiesPerSpawn = Mathf.Max(1, enemiesPerSpawn);
        maxActiveEnemies = Mathf.Max(1, maxActiveEnemies);
        spawnRadius = Mathf.Max(0.1f, spawnRadius);
        groundEdgePadding = Mathf.Max(0f, groundEdgePadding);
        recycleMargin = Mathf.Max(0f, recycleMargin);

        spawnTimer = 0f;
        totalSpawned = 0;

        Debug.Log($"敵人持續生成已啟動：每 {spawnInterval:0.##} 秒生成 {enemiesPerSpawn} 隻，場上上限 {maxActiveEnemies} 隻。", this);
    }

    private void Update()
    {
        if (!groundController.TryGetActiveGroundBounds(out Bounds activeArea))
        {
            return;
        }

        RecycleOutsideArea(activeArea);

        if (GameManager.Instance == null || GameManager.Instance.IsGameOver || player.IsDead)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
        {
            return;
        }

        spawnTimer -= spawnInterval;
        SpawnEnemies(activeArea);
    }

    private void SpawnEnemies(Bounds activeArea)
    {
        int slotsRemaining = maxActiveEnemies - activeEnemies.Count;

        if (slotsRemaining <= 0)
        {
            return;
        }

        if (!TryGetSpawnRadius(activeArea, out float effectiveRadius))
        {
            return;
        }

        int spawnCount = Mathf.Min(enemiesPerSpawn, slotsRemaining);
        Vector3 playerPosition = player.transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomEdgePosition(
                playerPosition,
                activeArea.center.y,
                effectiveRadius);

            EnemyPoolMember enemy = enemyPool.Spawn(spawnPosition);

            if (enemy == null)
            {
                if (!capacityWarningLogged)
                {
                    Debug.LogWarning("敵人物件池目前無可用敵人，請檢查 Max Capacity 或等待敵人回收。", this);
                    capacityWarningLogged = true;
                }

                break;
            }

            capacityWarningLogged = false;
            activeEnemies.Add(enemy);
            totalSpawned++;
        }
    }

    private bool TryGetSpawnRadius(Bounds activeArea, out float effectiveRadius)
    {
        Vector3 playerPosition = player.transform.position;

        float availableLeft = playerPosition.x - (activeArea.min.x + groundEdgePadding);
        float availableRight = activeArea.max.x - groundEdgePadding - playerPosition.x;
        float availableBottom = playerPosition.z - (activeArea.min.z + groundEdgePadding);
        float availableTop = activeArea.max.z - groundEdgePadding - playerPosition.z;

        effectiveRadius = Mathf.Min(
            spawnRadius,
            availableLeft,
            availableRight,
            availableBottom,
            availableTop);

        return effectiveRadius > 0.1f;
    }

    private Vector3 GetRandomEdgePosition(
        Vector3 playerPosition,
        float groundY,
        float radius)
    {
        float x = playerPosition.x;
        float z = playerPosition.z;

        switch (Random.Range(0, 4))
        {
            case 0:
                x -= radius;
                z += Random.Range(-radius, radius);
                break;

            case 1:
                x += radius;
                z += Random.Range(-radius, radius);
                break;

            case 2:
                x += Random.Range(-radius, radius);
                z -= radius;
                break;

            default:
                x += Random.Range(-radius, radius);
                z += radius;
                break;
        }

        return new Vector3(x, groundY + spawnHeight, z);
    }

    private void RecycleOutsideArea(Bounds activeArea)
    {
        float maxX = activeArea.extents.x + recycleMargin;
        float maxZ = activeArea.extents.z + recycleMargin;

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            EnemyPoolMember enemy = activeEnemies[i];

            if (enemy == null || enemy.IsAvailable || !enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            Vector3 enemyPosition = enemy.transform.position;
            float deltaX = Mathf.Abs(enemyPosition.x - activeArea.center.x);
            float deltaZ = Mathf.Abs(enemyPosition.z - activeArea.center.z);

            if (deltaX <= maxX && deltaZ <= maxZ)
            {
                continue;
            }

            enemy.ReturnToPool();
            activeEnemies.RemoveAt(i);
        }
    }
}
