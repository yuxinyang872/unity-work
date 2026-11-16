using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class InfiniteGroundController : MonoBehaviour
{
    private sealed class GroundTile
    {
        public GameObject GameObject;
        public Transform Transform;
        public GroundObstacleNode[] ObstacleNodes;
    }

    [Header("地板設定")]
    [SerializeField, Tooltip("用來重複排列的地板 Prefab。")]
    private GameObject groundPrefab;

    [SerializeField, Min(0.1f), Tooltip("單一地板在 XZ 平面上的邊長，必須與 Prefab 實際尺寸一致。")]
    private float tileSize = 10f;

    [SerializeField, Min(1), Tooltip("玩家周圍需要保留的內圈地板半徑。1 代表 3x3。")]
    private int viewRadius = 1;

    [SerializeField, Min(1), Tooltip("在內圈之外額外預先載入的地板圈數，建議至少 1 圈。")]
    private int preloadRings = 1;

    [SerializeField, Tooltip("地板所在的世界座標 Y。")]
    private float groundY;

    [Header("追蹤目標")]
    [SerializeField, Tooltip("要追蹤的玩家 Transform。")]
    private Transform target;

    private readonly Dictionary<Vector2Int, GroundTile> loadedTiles = new();
    private readonly Stack<GroundTile> availableTiles = new();
    private readonly List<Vector2Int> cellsToRelease = new();

    private Vector2Int currentCenterCell = new(int.MinValue, int.MinValue);
    private int loadRadius;

    public int LoadedTileCount => loadedTiles.Count;
    public int TotalTileCount => loadedTiles.Count + availableTiles.Count;

    private void Start()
    {
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }

        loadRadius = viewRadius + preloadRings;
        CreateGroundTiles();
        RefreshGround(true);
    }

    private void LateUpdate()
    {
        RefreshGround(false);
    }

    private void OnValidate()
    {
        tileSize = Mathf.Max(0.1f, tileSize);
        viewRadius = Mathf.Max(1, viewRadius);
        preloadRings = Mathf.Max(1, preloadRings);
    }

    private bool ValidateConfiguration()
    {
        if (groundPrefab == null || target == null)
        {
            Debug.LogError("無限地板初始化失敗，請指定地板 Prefab 與玩家。", this);
            return false;
        }

        return true;
    }

    private void CreateGroundTiles()
    {
        int width = loadRadius * 2 + 1;
        int tileCount = width * width;

        for (int i = 0; i < tileCount; i++)
        {
            GameObject ground = Instantiate(groundPrefab, transform);
            ground.name = $"Ground_{i:00}";

            GroundTile tile = new()
            {
                GameObject = ground,
                Transform = ground.transform,
                ObstacleNodes = ground.GetComponentsInChildren<GroundObstacleNode>(true)
            };

            ground.SetActive(false);
            availableTiles.Push(tile);
        }

        Debug.Log($"無限地板已建立 {tileCount} 個地板物件，預載圈數為 {preloadRings}。", this);
    }

    private void RefreshGround(bool forceRefresh)
    {
        Vector2Int centerCell = GetCell(target.position);

        if (!forceRefresh && centerCell == currentCenterCell)
        {
            return;
        }

        currentCenterCell = centerCell;
        ReleaseOutsideTiles(centerCell);

        for (int z = -loadRadius; z <= loadRadius; z++)
        {
            for (int x = -loadRadius; x <= loadRadius; x++)
            {
                Vector2Int cell = new(centerCell.x + x, centerCell.y + z);

                if (loadedTiles.ContainsKey(cell))
                {
                    continue;
                }

                AssignTile(cell);
            }
        }
    }

    private void ReleaseOutsideTiles(Vector2Int centerCell)
    {
        cellsToRelease.Clear();

        foreach (KeyValuePair<Vector2Int, GroundTile> pair in loadedTiles)
        {
            Vector2Int cell = pair.Key;

            if (Mathf.Abs(cell.x - centerCell.x) <= loadRadius &&
                Mathf.Abs(cell.y - centerCell.y) <= loadRadius)
            {
                continue;
            }

            cellsToRelease.Add(cell);
        }

        foreach (Vector2Int cell in cellsToRelease)
        {
            GroundTile tile = loadedTiles[cell];
            tile.GameObject.SetActive(false);
            loadedTiles.Remove(cell);
            availableTiles.Push(tile);
        }
    }

    private void AssignTile(Vector2Int cell)
    {
        if (availableTiles.Count == 0)
        {
            Debug.LogError("地板物件池不足，無法配置新地板。", this);
            return;
        }

        GroundTile tile = availableTiles.Pop();
        tile.Transform.position = new Vector3(
            cell.x * tileSize,
            groundY,
            cell.y * tileSize);

        // 先在預載範圍內放置障礙物，完成後才啟用地板。
        foreach (GroundObstacleNode obstacleNode in tile.ObstacleNodes)
        {
            if (obstacleNode != null)
            {
                obstacleNode.PrepareForCell(cell, tileSize, target.position);
            }
        }

        loadedTiles.Add(cell, tile);
        tile.GameObject.SetActive(true);
    }

    private Vector2Int GetCell(Vector3 worldPosition)
    {
        float halfTile = tileSize * 0.5f;

        return new Vector2Int(
            Mathf.FloorToInt((worldPosition.x + halfTile) / tileSize),
            Mathf.FloorToInt((worldPosition.z + halfTile) / tileSize));
    }

    // 保留原 API：敵人生成與回收沿用內圈地板範圍，不把預載圈當成戰鬥範圍。
    public bool TryGetActiveGroundBounds(out Bounds bounds)
    {
        bounds = default;

        if (target == null)
        {
            return false;
        }

        Vector2Int centerCell = GetCell(target.position);
        float sideLength = (viewRadius * 2 + 1) * tileSize;

        Vector3 center = new(
            centerCell.x * tileSize,
            groundY,
            centerCell.y * tileSize);

        bounds = new Bounds(
            center,
            new Vector3(sideLength, 0f, sideLength));

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null)
        {
            return;
        }

        Vector2Int centerCell = GetCell(target.position);
        Vector3 center = new(
            centerCell.x * tileSize,
            groundY + 0.05f,
            centerCell.y * tileSize);

        float visibleWidth = (viewRadius * 2 + 1) * tileSize;
        int previewRadius = viewRadius + preloadRings;
        float preloadWidth = (previewRadius * 2 + 1) * tileSize;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(visibleWidth, 0.1f, visibleWidth));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(preloadWidth, 0.2f, preloadWidth));
    }
#endif
}
