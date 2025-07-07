using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGameLevel : MonoBehaviour
{   //1.Open Starter Project
    //2.Create a Tilemap Game Level Class 
    Tilemap map;// 
  
    public Vector2Int mapSizeTiles = new Vector2Int(10, 10);

    public float chanceToSpawnFloor = 0.75f;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase sandTile;    // 新增：沙地
    [SerializeField] private TileBase waterTile;   // 新增：水面

    [SerializeField] private float floorCost = 1f; // 默认地面通行成本
    [SerializeField] private float sandCost = 2f; // 沙地通行成本
    [SerializeField] private float waterCost = 3f; // 水面通行成本

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
            { Vector3Int tilePos = new Vector3Int(x, y, 0);//Creates a tilemap-compatible coordinate for the tile at position (x, y).
                float n = Mathf.PerlinNoise(x * perlinScale, y * perlinScale);
                if (n < chanceToSpawnFloor)
                {
                    map.SetTile(tilePos, floorTile);
                }
                else if (n < chanceToSpawnFloor + 0.1f)
                {
                    map.SetTile(tilePos, sandTile);
                }
                else
                {
                    map.SetTile(tilePos, waterTile);
                }
            }
        }

    }
    //7.Write Helpful Utility Functions For the 
 
    //Determines whether the tile at the given coordinates is walkable (i.e., has a tile)
    public bool IsTraversible(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        return tile != null;
    }

    // Gets a reference to the tile at the specified coordinates
    public TileBase GetTile(int x, int y)
    {
        return map.GetTile(new Vector3Int(x, y, 0));
    }

    // Gets the world position of the center point of the specified tile (used for drawing or movement targets)
    public Vector3 GetTileCenter(int x, int y)
    {
        return map.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    // Gets the bounds of the entire tilemap (used for loops or range checks)
    public BoundsInt GetBounds()
    {
        return map.cellBounds;
    }

    // Gets the cost to enter a specific tile (can be customized for different tile types)
    public float GetCostToEnterTile(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        if (tile == floorTile) return floorCost;
        if (tile == sandTile) return sandCost;
        if (tile == waterTile) return waterCost;
        return float.PositiveInfinity; // 不可通行
    }

    // 8. Create a Function to Query for the Adjacency of a Tile
    // Returns a list of walkable adjacent tile coordinates (up, down, left, right)
    public List<Vector2Int> GetAdjacentTiles(int x, int y)
    {
        List<Vector2Int> adjacentTiles = new List<Vector2Int>();

        // Four directions: up, down, left, right
        Vector2Int[] directions = new Vector2Int[]
        {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
        new Vector2Int(1, 0)    // Right
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
    //9.Draw Tile Connections 
    /*private void OnDrawGizmos()
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
                    // Draw a green sphere: represents a walkable tile
                    Vector3 center = GetTileCenter(x, y);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(center, 0.1f);

                    // Get adjacent tiles and draw red lines to connect them
                    List<Vector2Int> neighbors = GetAdjacentTiles(x, y);
                    foreach (var neighbor in neighbors)
                    {
                        Vector3 neighborCenter = GetTileCenter(neighbor.x, neighbor.y);
                        Gizmos.color = Color.red;
                        //Gizmos.DrawLine(center, neighborCenter);
                    }
                }
            }
        }
    }*/
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || map == null) return;
        BoundsInt bounds = map.cellBounds;

        // 提前构造好样式
#if UNITY_EDITOR
        GUIStyle style = new GUIStyle()
        {
            fontSize = 12,
            normal = { textColor = Color.black }
        };
#endif

        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (IsTraversible(x, y))
                {
                    // 先画球代表可走
                    Vector3 center = GetTileCenter(x, y);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(center, 0.1f);

                    // （可选）画相邻连线
                    List<Vector2Int> neighbors = GetAdjacentTiles(x, y);
                    foreach (var n in neighbors)
                    {
                        Vector3 nc = GetTileCenter(n.x, n.y);
                        Gizmos.color = Color.red;
                        //Gizmos.DrawLine(center, nc);
                    }

                    // **在这里** 显示每格通行成本
#if UNITY_EDITOR
                    float stepCost = GetCostToEnterTile(x, y);
                    Handles.Label(center + Vector3.up * 0.2f,
                                  stepCost.ToString("F0"),
                                  style);
#endif
                }
            }
    }
}
