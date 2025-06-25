using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public float GetTotalCostToReach(Vector2Int node)//返回该点的总代价。
    {
        if (nodes.ContainsKey(node))
            return nodes[node].gCost;
        return float.PositiveInfinity;
    }

    public void UpdateBestWayToReachTile(Vector2Int origin, Vector2Int destination, float cost)//更新邻居的 gCost 和 previous。

    {
        nodes[destination] = new DijkstraNodeData(cost, origin);
    }

    public bool IsDiscovered(Vector2Int node)//管理节点状态。
    {
        return visited.Contains(node);
    }

    public bool IsVisited(Vector2Int node)
    {
        return visited.Contains(node);
    }

    public void MoveToVisitedSet(Vector2Int node)
    {
        visited.Add(node);
    }

    public Tuple<Vector2Int, float> GetLowestCostInUnvisited()
    {
        Vector2Int bestNode = new Vector2Int(int.MaxValue, int.MaxValue);
        float bestCost = float.PositiveInfinity;

        foreach (Vector2Int node in nodeData.Keys)
        {
            if (!visited.Contains(node))
            {
                float cost = nodeData[node].gCost;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestNode = node;
                }
            }
        }

        return new Tuple<Vector2Int, float>(bestNode, bestCost);
    }

    public void DijkstraIteration()
    {
        // 1. Get the node with the lowest cost from the unvisited set and set it as 'current'. We will visit this node.
        currentNode = GetLowestCostInUnvisited().Item1;

        // Log and draw a circle in the scene to visualize what we are doing
        Debug.Log("Visiting: " + currentNode + ", cost: " + nodes[currentNode].gCost);
        DebugDrawing.DrawCircle(tilemap.GetTileCenter(currentNode.x, currentNode.y), Quaternion.AngleAxis(90, Vector3.forward), 0.6f, 16, Color.yellow, 0.1f, false);

        // 2. Iterate through all adjacent and walkable tiles connected to 'current'
        foreach (Vector2Int connected in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
        {
            float costToReachConnected = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(connected.x, connected.y);

            if (!IsDiscovered(connected))
            {
                Debug.Log("Discovered: " + connected + ", cost: " + costToReachConnected);
                UpdateBestWayToReachTile(currentNode, connected, costToReachConnected);
                frontier.Enqueue(connected, costToReachConnected);
            }
            else if (costToReachConnected < nodeData[connected].gCost)
            {
                UpdateBestWayToReachTile(currentNode, connected, costToReachConnected);
                frontier.Enqueue(connected, costToReachConnected);
            }
        }

        MoveToVisitedSet(currentNode);
    }
    public IEnumerator StepThroughDijkstra(float delaySeconds = 0.5f)
    {
        while (!IsComplete())
        {
            DijkstraIteration();
            yield return new WaitForSeconds(delaySeconds);
        }

        if (IsSolved())
        {
            ReconstructPath();
            Debug.Log("✅ Path found!");
        }
        else
        {
            Debug.LogWarning("❌ No path found.");
        }
    }
    private void ReconstructPath()
    {
        solution.Clear();
        Vector2Int step = end;

        while (step != start)
        {
            solution.Add(step);
            step = nodeData[step].previous;
        }

        solution.Add(start);
        solution.Reverse();
        pathFound = true;
    }
    
    private bool IsSolved()
    {
        return IsVisited(end);
    }

    private bool IsComplete()
    {
        return IsSolved() || GetLowestCostInUnvisited().Item2 == float.PositiveInfinity;
    }
    void GenerateSolution()
    {
        // 1. 如果没有成功找到终点，就抛出异常（不能生成路径）
        if (!IsSolved())
        {
            throw new Exception("Not solved! Cannot generate solution");
        }

        // 2. 初始化一条空的路径列表 solution
        solution = new List<Vector2Int>();

        // 3. 从终点开始，反向回溯
        Vector2Int currentNode = end;
        do
        {
            // 3.1 把当前节点加到路径里
            solution.Add(currentNode);

            // 3.2 找到这个节点是从哪个前一个节点走来的（nodes 中记录了 previous）
            currentNode = nodes[currentNode].previous;

        } while (currentNode != start); // 如果没回溯到起点，就继续

        // 4. 最后别忘了把起点也加入路径
        solution.Add(start);

        // 5. 整个路径是从终点往起点加的，要翻转一下变成从起点到终点
        solution.Reverse();
    }
    public IEnumerator DijkstraSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {
        start = origin;
        end = destination;
        solution = new List<Vector2Int>();
        nodeData = new Dictionary<Vector2Int, DijkstraNodeData>();
        frontier = new PriorityQueue<Vector2Int>();
        visited = new HashSet<Vector2Int>();
        pathFound = false;

        nodeData[origin] = new DijkstraNodeData(0f, origin);
        frontier.Enqueue(origin, 0f);

        while (!IsComplete())
        {
            if (frontier.Count == 0)
                break;

            currentNode = frontier.Dequeue();

            if (visited.Contains(currentNode))
                continue;

            visited.Add(currentNode);

            foreach (var neighbor in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
            {
                float newCost = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(neighbor.x, neighbor.y);

                if (!nodeData.ContainsKey(neighbor) || newCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData(newCost, currentNode);
                    frontier.Enqueue(neighbor, newCost);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (IsSolved())
        {
           /* solution.Clear();
            Vector2Int step = end;

            while (step != start)
            {
                solution.Add(step);
                step = nodeData[step].previous;
            }

            solution.Add(start);
            solution.Reverse();*/
            pathFound = true;
            Debug.Log("✅ 路径找到！");
            GenerateSolution();
        }
        else
        {
            Debug.LogWarning("❌ 未找到路径");
        }
    }


    void OnDrawGizmos()
    {
        if (tilemap == null || nodeData == null)
            return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;

        Vector3 startNodeWorldspace = tilemap.GetTileCenter(start.x, start.y);
        Vector3 endNodeWorldspace = tilemap.GetTileCenter(end.x, end.y);

        // Draw Start node and label
        style.normal.textColor = new Color(0.05f, 0.8f, 0.05f, 1.0f);
        Handles.Label(startNodeWorldspace + Vector3.up * 0.4f, "START", style);
        DebugDrawing.DrawCircle(startNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 8, Color.green, Time.deltaTime, false);

        // Draw End node and label
        style.normal.textColor = new Color(0.8f, 0.05f, 0.05f, 1.0f);
        Handles.Label(endNodeWorldspace + Vector3.up * 0.4f, "END", style);
        DebugDrawing.DrawCircle(endNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 5, Color.red, Time.deltaTime, false);

        if (nodes != null)
        {
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
            style.normal.textColor = new Color(0.05f, 0.05f, 0.05f, 1.0f);

            foreach (KeyValuePair<Vector2Int, DijkstraNodeData> pair in nodes)
            {
                Vector2Int nodePos = pair.Key;
                Vector2Int prev = pair.Value.previous;
                float cost = pair.Value.gCost;

                Vector3 nodePosWorldspace = tilemap.GetTileCenter(nodePos.x, nodePos.y);
                Vector3 prevNodePosWorldspace = tilemap.GetTileCenter(prev.x, prev.y);

                // Draw each edge found in the algorithm
                Debug.DrawLine(prevNodePosWorldspace, nodePosWorldspace, Color.HSVToRGB(cost / 10, 1, 0.8f), Time.deltaTime);

                // Label each node with its current G-Cost
                Handles.Label(nodePosWorldspace + Vector3.up * 0.4f, cost.ToString("F0"), style);
            }
        }
    }
}
