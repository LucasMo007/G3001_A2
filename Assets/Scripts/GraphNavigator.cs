using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TilemapGameLevel))]
public class GraphNavigator : MonoBehaviour
{
    private TilemapGameLevel level;
    private Graph<Vector2Int> graph;

    private void Awake()
    {
        level = GetComponent<TilemapGameLevel>();
        //graph = level.ToGraph();
    }
    private void Start()
    {
        // 保证迷宫已生成完毕后再构建图
        graph = level.ToGraph();
    }


    /// <summary>
    /// 寻路：可切换 Dijkstra 或 A* 算法。
    /// </summary>
    public List<Vector2Int> FindTilePath(Vector2Int start, Vector2Int end, bool useAStar = false)
    {
        if (useAStar) return AStar(start, end);
        return graph.Dijkstra(start, end) ?? new List<Vector2Int>();
    }

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int goal)
    {
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { { start, 0f } };
        var fScore = new Dictionary<Vector2Int, float> { { start, Heuristic(start, goal) } };
        var closed = new HashSet<Vector2Int>();

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(v => fScore.ContainsKey(v) ? fScore[v] : float.PositiveInfinity).First();
            if (current == goal)
                return Reconstruct(cameFrom, current);
            openSet.Remove(current);
            closed.Add(current);
            foreach (var n in level.GetAdjacentTiles(current.x, current.y))
            {
                if (!level.IsTraversable(n.x, n.y)) continue;
                float tg = gScore[current] + level.GetCostToEnterTile(n.x, n.y);
                if (closed.Contains(n) && tg >= (gScore.ContainsKey(n) ? gScore[n] : float.PositiveInfinity)) continue;
                if (!openSet.Contains(n)) openSet.Add(n);
                if (!gScore.ContainsKey(n) || tg < gScore[n])
                {
                    cameFrom[n] = current;
                    gScore[n] = tg;
                    fScore[n] = tg + Heuristic(n, goal);
                }
            }
        }
        return new List<Vector2Int>();
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}

