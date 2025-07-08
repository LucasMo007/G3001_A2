/*using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages tilemap generation, rendering, and navigation utilities.
/// Supports conversion to a generic graph structure based on Dijkstra's algorithm for AI pathfinding.
/// </summary>
public class TilemapGameLevel : MonoBehaviour
{
    // Reference to the Tilemap component
    private Tilemap map;

    // Map configuration
    public Vector2Int mapSizeTiles = new Vector2Int(10, 10);
    public float chanceToSpawnFloor = 0.75f;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase sandTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private float floorCost = 1f;
    [SerializeField] private float sandCost = 2f;
    [SerializeField] private float waterCost = 3f;
    public float perlinScale = 0.1f;

    private void Start()
    {
        map = GetComponent<Tilemap>();
        GenerateMap();
        Debug.Log("Tile at (0,0) walkable? " + IsTraversable(0, 0));
    }

    /// <summary>
    /// Sets the probability of spawning floor tiles.
    /// </summary>
    public void SetChanceToSpawnFloor(float chance)
    {
        chanceToSpawnFloor = chance;
    }

    /// <summary>
    /// Generates the tilemap using Perlin noise.
    /// </summary>
    public void GenerateMap()
    {
        for (int x = 0; x < mapSizeTiles.x; x++)
        {
            for (int y = 0; y < mapSizeTiles.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                float noiseValue = Mathf.PerlinNoise(x * perlinScale, y * perlinScale);
                if (noiseValue < chanceToSpawnFloor)
                {
                    map.SetTile(tilePos, floorTile);
                }
                else if (noiseValue < chanceToSpawnFloor + 0.1f)
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

    /// <summary>
    /// Checks if the tile at the given coordinates is traversable (i.e., has a tile).
    /// </summary>
    public bool IsTraversable(int x, int y)
    {
        return GetTile(x, y) != null;
    }

    /// <summary>
    /// Retrieves the TileBase at the given coordinates.
    /// </summary>
    public TileBase GetTile(int x, int y)
    {
        return map.GetTile(new Vector3Int(x, y, 0));
    }

    /// <summary>
    /// Gets the world-space center of the tile at the given coordinates.
    /// </summary>
    public Vector3 GetTileCenter(int x, int y)
    {
        return map.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    /// <summary>
    /// Gets the bounds of the tilemap.
    /// </summary>
    public BoundsInt GetBounds()
    {
        return map.cellBounds;
    }

    /// <summary>
    /// Gets the cost to enter the tile at the given coordinates.
    /// </summary>
    public float GetCostToEnterTile(int x, int y)
    {
        TileBase tile = GetTile(x, y);
        if (tile == floorTile) return floorCost;
        if (tile == sandTile) return sandCost;
        if (tile == waterTile) return waterCost;
        return float.PositiveInfinity;
    }

    /// <summary>
    /// Gets a list of traversable neighbor coordinates (up, down, left, right) for the tile at the given coordinates.
    /// </summary>
    public List<Vector2Int> GetAdjacentTiles(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            if (IsTraversable(nx, ny))
            {
                neighbors.Add(new Vector2Int(nx, ny));
            }
        }
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || map == null) return;
        BoundsInt bounds = GetBounds();
        GUIStyle style = new GUIStyle() { fontSize = 12, normal = { textColor = Color.black } };

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (!IsTraversable(x, y)) continue;

                Vector3 center = GetTileCenter(x, y);
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(center, 0.1f);

                List<Vector2Int> neighbors = GetAdjacentTiles(x, y);
                foreach (Vector2Int neighbor in neighbors)
                {
                    Vector3 neighborCenter = GetTileCenter(neighbor.x, neighbor.y);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(center, neighborCenter);
                }

#if UNITY_EDITOR
                float cost = GetCostToEnterTile(x, y);
                Handles.Label(center + Vector3.up * 0.2f, cost.ToString("F0"), style);
#endif
            }
        }
    }

    /// <summary>
    /// Converts the current tilemap to a generic Graph<Vector2Int>,
    /// where each vertex represents a traversable tile and edges are weighted by the cost to enter adjacent tiles.
    /// </summary>
    public Graph<Vector2Int> ToGraph()
    {
        Graph<Vector2Int> graph = new Graph<Vector2Int>();
        BoundsInt bounds = GetBounds();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (!IsTraversable(x, y)) continue;
                Vector2Int position = new Vector2Int(x, y);
                graph.AddVertex(position);
                foreach (Vector2Int neighbor in GetAdjacentTiles(x, y))
                {
                    float weight = GetCostToEnterTile(neighbor.x, neighbor.y);
                    graph.AddEdge(position, neighbor, weight, directed: false);
                }
            }
        }

        return graph;
    }
}*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages tilemap generation, rendering, and navigation utilities.
/// Supports maze generation with loops and three tile types (floor, sand, water),
/// using a configurable chance to spawn floor segments.
/// </summary>
public class TilemapGameLevel : MonoBehaviour
{
    // Reference to the Tilemap component
    private Tilemap map;

    [Header("Map Size (odd values recommended)")]
    public Vector2Int mapSizeTiles = new Vector2Int(21, 21);

    [Header("Tile Types & Spawn Probabilities")]
    [Tooltip("Tile used for floor/corridor segments.")]
    public TileBase floorTile;
    [Tooltip("Tile used for sand segments.")]
    public TileBase sandTile;
    [Tooltip("Tile used for water segments.")]
    public TileBase waterTile;

    [Tooltip("Probability [0..1] to spawn a floor tile at each corridor cell.")]
    [Range(0f, 1f)]
    public float chanceToSpawnFloor = 0.75f;

    [Header("Maze Generation Settings")]
    [Tooltip("Number of extra loop passages to carve in the maze.")]
    public int mazeLoopFactor = 30;

    [Header("Tile Movement Costs")]
    public float floorCost = 1f;
    public float sandCost = 2f;
    public float waterCost = 3f;
    private void Awake()
    //{
        // ensures map is ready for any Awake-based callers
        //map = GetComponent<Tilemap>();

   // }
    {
    map = GetComponent<Tilemap>() 
        ?? GetComponentInChildren<Tilemap>();
    if (map == null)
        Debug.LogError("TilemapGameLevel: 未在此物体或其子物体中找到 Tilemap 组件！");
    }

private void Start()
    {
       // map = GetComponent<Tilemap>();
        GenerateMaze();
        Debug.Log($"Maze generated ({mapSizeTiles.x}x{mapSizeTiles.y}), loops: {mazeLoopFactor}");
    }

    /// <summary>
    /// Generates a random maze using recursive backtracking,
    /// adds extra loops, and populates corridors with one of three tile types.
    /// </summary>
    public void GenerateMaze()
    {
        map.ClearAllTiles();
        int width = (mapSizeTiles.x % 2 == 0 ? mapSizeTiles.x - 1 : mapSizeTiles.x);
        int height = (mapSizeTiles.y % 2 == 0 ? mapSizeTiles.y - 1 : mapSizeTiles.y);

        bool[,] visited = new bool[width, height];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int start = new Vector2Int(1, 1);
        visited[start.x, start.y] = true;
        CarveTile(start);
        stack.Push(start);

        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(0, 2), new Vector2Int(0, -2),
            new Vector2Int(2, 0), new Vector2Int(-2, 0)
        };

        // Carve initial perfect maze
        while (stack.Count > 0)
        {
            Vector2Int cell = stack.Pop();
            List<Vector2Int> neighbors = new List<Vector2Int>();
            foreach (var d in dirs)
            {
                int nx = cell.x + d.x;
                int ny = cell.y + d.y;
                if (nx > 0 && nx < width && ny > 0 && ny < height && !visited[nx, ny])
                    neighbors.Add(new Vector2Int(nx, ny));
            }
            if (neighbors.Count > 0)
            {
                stack.Push(cell);
                Vector2Int next = neighbors[Random.Range(0, neighbors.Count)];
                visited[next.x, next.y] = true;
                Vector2Int between = new Vector2Int((cell.x + next.x) / 2, (cell.y + next.y) / 2);
                CarveTile(between);
                CarveTile(next);
                stack.Push(next);
            }
        }

        // Add random loops
        for (int i = 0; i < mazeLoopFactor; i++)
        {
            int x = Random.Range(1, width);
            int y = Random.Range(1, height);
            if ((x % 2 == 1) && (y % 2 == 1))
            {
                var d = dirs[Random.Range(0, dirs.Length)];
                int bx = x + d.x / 2;
                int by = y + d.y / 2;
                Vector3Int wallPos = new Vector3Int(bx, by, 0);
                if (map.GetTile(wallPos) == null)
                    CarveTile(new Vector2Int(bx, by));
            }
        }
    }

    /// <summary>
    /// Carves a corridor cell and assigns one of three tile types based on spawn probability.
    /// </summary>
    private void CarveTile(Vector2Int pos)
    {
        TileBase chosen = ChooseTileType();
        map.SetTile(new Vector3Int(pos.x, pos.y, 0), chosen);
    }

    /// <summary>
    /// Randomly picks floor, sand, or water tile according to chanceToSpawnFloor.
    /// Remainder is split evenly between sand and water.
    /// </summary>
    private TileBase ChooseTileType()
    {
        float r = Random.value;
        if (r < chanceToSpawnFloor) return floorTile;
        float halfRem = (1f - chanceToSpawnFloor) * 0.5f;
        if (r < chanceToSpawnFloor + halfRem) return sandTile;
        return waterTile;
    }

    /// <summary>
    /// Checks if the tile at the given coordinates is traversable (non-null).
    /// </summary>
    public bool IsTraversable(int x, int y)
    {
        return map.GetTile(new Vector3Int(x, y, 0)) != null;
    }

    /// <summary>
    /// Gets the world-space center of the tile.
    /// </summary>
    public Vector3 GetTileCenter(int x, int y)
    {
        return map.GetCellCenterWorld(new Vector3Int(x, y, 0));
    }

    /// <summary>
    /// Returns the bounds of the tilemap.
    /// </summary>
    public BoundsInt GetBounds()
    {
        return map.cellBounds;
    }

    /// <summary>
    /// Returns movement cost based on actual tile type.
    /// </summary>
    public float GetCostToEnterTile(int x, int y)
    {
        var tile = map.GetTile(new Vector3Int(x, y, 0));
        if (tile == floorTile) return floorCost;
        if (tile == sandTile) return sandCost;
        if (tile == waterTile) return waterCost;
        return float.PositiveInfinity;
    }

    /// <summary>
    /// Gets traversable neighbors (up/down/left/right).
    /// </summary>
    public List<Vector2Int> GetAdjacentTiles(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] dirs4 = new Vector2Int[]
        {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };
        foreach (var d in dirs4)
        {
            int nx = x + d.x, ny = y + d.y;
            if (IsTraversable(nx, ny))
                neighbors.Add(new Vector2Int(nx, ny));
        }
        return neighbors;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || map == null) return;
        BoundsInt b = GetBounds();
        for (int x = b.xMin; x < b.xMax; x++)
            for (int y = b.yMin; y < b.yMax; y++)
                if (IsTraversable(x, y))
                {
                    Vector3 c = GetTileCenter(x, y);
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(c, 0.1f);
                }
    }

    /// <summary>
    /// Converts the maze to a Graph<Vector2Int> for pathfinding.
    /// </summary>
    public Graph<Vector2Int> ToGraph()
    {
        var graph = new Graph<Vector2Int>();
        var b = GetBounds();
        for (int x = b.xMin; x < b.xMax; x++)
            for (int y = b.yMin; y < b.yMax; y++)
                if (IsTraversable(x, y))
                {
                    var p = new Vector2Int(x, y);
                    graph.AddVertex(p);
                    foreach (var n in GetAdjacentTiles(x, y))
                        graph.AddEdge(p, n, GetCostToEnterTile(n.x, n.y), directed: false);
                }
        return graph;
    }
}
