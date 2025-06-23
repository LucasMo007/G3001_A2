using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    private Dictionary<Vector2Int, DijkstraNodeData> nodes = new();
    public struct DijkstraNodeData
    {
        public float gCost;
        public Vector2Int previous;

        public DijkstraNodeData(float cost, Vector2Int from)
        {
            gCost = cost;
            previous = from;
        }
    }

    [Header("Path Settings")]
    [SerializeField] private Vector2Int startCoord = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int endCoord = new Vector2Int(5, 5);

    private Vector2Int currentNode;

    // Core algorithm data
    private PriorityQueue<Vector2Int> frontier;
    private HashSet<Vector2Int> visited;
    private Dictionary<Vector2Int, DijkstraNodeData> nodeData;

    // Final solution path
    private List<Vector2Int> solution;

    // Tilemap reference
    private TilemapGameLevel tilemap;

    // State
    private bool pathFound = false;

    void Awake()
    {
        tilemap = GetComponent<TilemapGameLevel>();
    }

    // Called externally (like by MouseDisplay.cs)
    internal void FindPathDebugging()
    {
        pathFound = false;
        StopAllCoroutines();
        StartCoroutine(DijkstraSearchCoroutine(startCoord, endCoord));
    }
    public Vector2Int start
    {
        get => startCoord;
        set => startCoord = value;
    }

    public Vector2Int end
    {
        get => endCoord;
        set => endCoord = value;
    }
    public float GetTotalCostToReach(Vector2Int node)
    {
        if (nodes.ContainsKey(node))
        {
            return nodes[node].gCost;
        }
        else
        {
            return float.PositiveInfinity;
        }
    }
    public void UpdateBestWayToReachTile(Vector2Int origin, Vector2Int destination, float cost)
    {
        nodes[destination] = new DijkstraNodeData(cost, origin);
    }
    // 判断是否已经探索过
    public bool IsDiscovered(Vector2Int node)
    {
        return visited.Contains(node);
    }

    // 判断是否已经访问过（完全处理）
    public bool IsVisited(Vector2Int node)
    {
        return visited.Contains(node);
    }

    // 将节点从 unvisited 移到 visited
    public void MoveToVisitedSet(Vector2Int node)
    {
        visited.Add(node); // 你目前没有 unvisited 集合，所以只加 visited 即可
    }
    public IEnumerator DijkstraSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {
        frontier = new PriorityQueue<Vector2Int>();
        visited = new HashSet<Vector2Int>();
        nodeData = new Dictionary<Vector2Int, DijkstraNodeData>();
        solution = new List<Vector2Int>();

        nodeData[origin] = new DijkstraNodeData(0f, origin);
        frontier.Enqueue(origin, 0f);

        while (frontier.Count > 0)
        {
            currentNode = frontier.Dequeue();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            if (currentNode == destination)
            {
                solution.Clear();
                Vector2Int step = destination;

                while (step != origin)
                {
                    solution.Add(step);
                    step = nodeData[step].previous;
                }

                solution.Add(origin);
                solution.Reverse();
                Debug.Log("✅ 路径找到！");
                pathFound = true;
                yield break;
            }

            foreach (var neighbor in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
            {
                float newCost = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(neighbor.x, neighbor.y);

                if (!nodeData.ContainsKey(neighbor) || newCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData(newCost, currentNode);
                    frontier.Enqueue(neighbor, newCost);
                }
            }

            yield return new WaitForSeconds(0.1f); // 延迟可视化，调整速度
        }

        Debug.LogWarning("❌ 未找到路径");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || tilemap == null || nodeData == null)
            return;

        // 显示访问过的节点
        foreach (var tile in nodeData.Keys)
        {
            Vector3 worldPos = tilemap.GetTileCenter(tile.x, tile.y);
            Gizmos.color = visited.Contains(tile) ? Color.gray : Color.white;
            Gizmos.DrawSphere(worldPos, 0.1f);
        }

        // 起点和终点标记
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(tilemap.GetTileCenter(startCoord.x, startCoord.y), 0.15f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(tilemap.GetTileCenter(endCoord.x, endCoord.y), 0.15f);

        // 路径线段
        if (solution != null && solution.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < solution.Count; i++)
            {
                Vector3 from = tilemap.GetTileCenter(solution[i - 1].x, solution[i - 1].y);
                Vector3 to = tilemap.GetTileCenter(solution[i].x, solution[i].y);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}