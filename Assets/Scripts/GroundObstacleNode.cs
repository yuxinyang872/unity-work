using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GroundObstacleNode : MonoBehaviour
{
    [Header("障礙物種類")]
    [SerializeField, Tooltip("可隨機產生的障礙物 Prefab。每次配置地板時，從有效清單等機率選出一種。")]
    private List<GameObject> obstaclePrefabs = new();

    [Header("生成位置")]
    [SerializeField, Min(0f), Tooltip("障礙物中心與地板邊界保留的最小距離。請依障礙物實際尺寸調整。")]
    private float edgePadding = 1f;

    [SerializeField, Tooltip("障礙物相對於地板的高度。建議障礙物 Prefab 的 Pivot 位於底部。")]
    private float localHeight;

    [SerializeField, Min(0f), Tooltip("選用設定。避免在玩家附近產生障礙物。設為 0 可保留完全固定的世界格子配置。")]
    private float playerSafeRadius;

    [SerializeField, Min(1), Tooltip("尋找有效位置的最多嘗試次數。")]
    private int maxPlacementAttempts = 16;

    [Header("隨機設定")]
    [SerializeField, Tooltip("是否隨機旋轉障礙物的 Y 軸角度。")]
    private bool randomYaw = true;

    [SerializeField, Tooltip("固定隨機種子。相同世界格子與清單順序會選到相同種類及位置。")]
    private int randomSeed = 12345;

    private readonly List<GameObject> validPrefabs = new();
    private readonly Dictionary<GameObject, GameObject> cachedInstances = new();
    private GameObject activeObstacle;
    private bool warnedInvalidArea;

    private void Awake()
    {
        RefreshPrefabCandidates();
    }

    private void RefreshPrefabCandidates()
    {
        validPrefabs.Clear();

        foreach (GameObject prefab in obstaclePrefabs)
        {
            if (prefab != null && !validPrefabs.Contains(prefab))
            {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("障礙物節點沒有設定有效的障礙物 Prefab。", this);
        }
    }

    public void PrepareForCell(Vector2Int cell, float tileSize, Vector3 playerPosition)
    {
        if (validPrefabs.Count == 0)
        {
            DisableActiveObstacle();
            return;
        }

        float allowedHalfSize = tileSize * 0.5f - edgePadding;

        if (allowedHalfSize <= 0f)
        {
            DisableActiveObstacle();

            if (!warnedInvalidArea)
            {
                Debug.LogError("地板的障礙物生成範圍不足，請減小 Edge Padding。", this);
                warnedInvalidArea = true;
            }

            return;
        }

        warnedInvalidArea = false;

        int cellSeed = unchecked(
            randomSeed ^ (cell.x * 73856093) ^ (cell.y * 19349663));

        System.Random random = new(cellSeed);
        int prefabIndex = random.Next(validPrefabs.Count);
        GameObject selectedPrefab = validPrefabs[prefabIndex];

        if (!TryFindPosition(random, allowedHalfSize, playerPosition, out Vector3 localPosition))
        {
            DisableActiveObstacle();
            return;
        }

        if (!cachedInstances.TryGetValue(selectedPrefab, out GameObject instance) || instance == null)
        {
            instance = Instantiate(selectedPrefab, transform);
            instance.name = $"Obstacle_{selectedPrefab.name}";
            instance.SetActive(false);
            cachedInstances[selectedPrefab] = instance;
        }

        DisableActiveObstacle();

        instance.transform.localPosition = localPosition;
        float yaw = randomYaw ? (float)random.NextDouble() * 360f : 0f;
        instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        instance.SetActive(true);
        activeObstacle = instance;
    }

    private bool TryFindPosition(
        System.Random random,
        float allowedHalfSize,
        Vector3 playerPosition,
        out Vector3 localPosition)
    {
        localPosition = Vector3.zero;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            float x = Mathf.Lerp(
                -allowedHalfSize,
                allowedHalfSize,
                (float)random.NextDouble());

            float z = Mathf.Lerp(
                -allowedHalfSize,
                allowedHalfSize,
                (float)random.NextDouble());

            Vector3 candidate = new(x, localHeight, z);

            if (playerSafeRadius > 0f)
            {
                Vector3 worldPosition = transform.TransformPoint(candidate);
                Vector2 offset = new(
                    worldPosition.x - playerPosition.x,
                    worldPosition.z - playerPosition.z);

                if (offset.sqrMagnitude < playerSafeRadius * playerSafeRadius)
                {
                    continue;
                }
            }

            localPosition = candidate;
            return true;
        }

        return false;
    }

    private void DisableActiveObstacle()
    {
        if (activeObstacle == null)
        {
            return;
        }

        activeObstacle.SetActive(false);
        activeObstacle = null;
    }
}
