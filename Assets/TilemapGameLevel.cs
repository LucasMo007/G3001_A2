using System.Collections.Generic;
using TreeEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGameLevel : MonoBehaviour
{
    Tilemap map;
    [SerializeField] TileBase floorTile;
    public Vector2Int mapSizeTiles = new Vector2Int(10, 10);

    public float chanceToSpawnFloor = 0.75f;

    public float perlinScale = 0.1f;
    private void Start()
    {
        map = GetComponent<Tilemap>();
        //map.SetTile(new Vector3Int(0,0,0),floorTile);
        //map.SetTile(new Vector3Int(4,0,0),floorTile);
        GenerateMap();
        Debug.Log("Tile at (0,0) walkable? " + IsTraversible(0, 0));
    }
    public void SetChanceToSpawnFloor(float chance) 
    {
        chanceToSpawnFloor = chance;
    }
    public void GenerateMap()
    {
        //map.ClearAllTiles();
        for (int x = 0; x < mapSizeTiles.x; x++)
        {
            for (int y = 0; y < mapSizeTiles.y; y++)
            { Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (Mathf.PerlinNoise(x* perlinScale, y* perlinScale) < chanceToSpawnFloor)
                {

                    map.SetTile(tilePos, floorTile);

                }
                else
                {
                    map.SetTile(tilePos, null);
                }
            }
        }

    }
    // 判断指定坐标是否可通行（是否有 tile）
    public bool IsTraversible(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        return tile != null;
    }

    // 获取指定 tile 的引用
    public TileBase GetTile(int x, int y)
    {
        return map.GetTile(new Vector3Int(x, y, 0));
    }

    // 获取指定 tile 的中心点世界坐标（用于绘图或角色移动目标）
    public Vector3 GetTileCenter(int x, int y)
    {
        return map.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    // 获取整个 tilemap 的边界（用于循环或范围判断）
    public BoundsInt GetBounds()
    {
        return map.cellBounds;
    }

    // 获取进入某 tile 的代价（可自定义不同 tile 类型的代价）
    public float GetCostToEnterTile(int x, int y)
    {
        return 1; // 默认统一代价为1，可扩展为根据tile类型判断
    }
    // 返回上下左右所有可通行的邻接 tile 坐标
    public List<Vector2Int> GetAdjacentTiles(int x, int y)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();

        // 四个方向：上下左右
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // 上
        new Vector2Int(0, -1),  // 下
        new Vector2Int(-1, 0),  // 左
        new Vector2Int(1, 0)    // 右
        };

        foreach (var dir in directions)
        {
            int newX = x + dir.x;
            int newY = y + dir.y;

            if (IsTraversible(newX, newY))
            {
                adjacentTiles.Add(new Vector2Int(newX, newY));
            }
        }

        return adjacentTiles;
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;
        if (map == null) return;

        BoundsInt bounds = map.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (IsTraversible(x, y))
                {
                    // 画圆点：表示这个 tile 是可通行的
                    Vector3 center = GetTileCenter(x, y);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(center, 0.1f);

                    // 获取邻接 tile，并连接线段
                    List<Vector2Int> neighbors = GetAdjacentTiles(x, y);
                    foreach (var neighbor in neighbors)
                    {
                        Vector3 neighborCenter = GetTileCenter(neighbor.x, neighbor.y);
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(center, neighborCenter);
                    }
                }
            }
        }
    }

}