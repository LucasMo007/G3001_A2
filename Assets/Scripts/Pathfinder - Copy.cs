
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [Header("Debug Step Delay (s)")]
    public float stepDelay = 1f;
    public float speedUpFactor = 0.1f;

    [Header("Safety Limits")]
    public int maxIterations = 500;

    [Header("Debug Visualization")]
    public bool showTileCosts = true;          // 显示路径探索累计总代价
    public bool showAllTileBaseCosts = false;  // 显示整个地图每个 tile 的静态进入代价

    public bool PathFound => pathFound;
    public List<Vector2Int> Solution => solution;

    private Dictionary<Vector2Int, DijkstraNodeData> nodeData;
    private int currentIterations = 0;

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

    private PriorityQueue<Vector2Int> frontier;
    private HashSet<Vector2Int> visited;
    private HashSet<Vector2Int> inFrontier;

    private List<Vector2Int> solution;

    [SerializeField] private TilemapGameLevel tilemap;
    private bool pathFound = false;

    void Awake()
    {
        tilemap = GetComponent<TilemapGameLevel>() ?? GetComponentInChildren<TilemapGameLevel>();
        if (tilemap == null)
            Debug.LogError("Pathfinder: 找不到 TilemapGameLevel，请在 Inspector 中拖入。");
    }

    
    internal void FindPathDebugging(bool useAStar = false)
    {
        pathFound = false;
        currentIterations = 0;
        StopAllCoroutines();
        if (useAStar)
            StartCoroutine(AStarSearchCoroutine(startCoord, endCoord));
        else
            StartCoroutine(DijkstraSearchCoroutine(startCoord, endCoord));
    }
    public Vector2Int start { get => startCoord; set => startCoord = value; }
    public Vector2Int end { get => endCoord; set => endCoord = value; }

    public float GetTotalCostToReach(Vector2Int node)
    {
        return nodeData.TryGetValue(node, out var data) ? data.gCost : float.PositiveInfinity;
    }

    public IEnumerator DijkstraSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {
        if (tilemap == null)
        {
            Debug.LogError("TilemapGameLevel 未设置，无法执行路径搜索。");
            yield break;
        }

        start = origin;
        end = destination;
        solution = new List<Vector2Int>();
        nodeData = new Dictionary<Vector2Int, DijkstraNodeData>();
        frontier = new PriorityQueue<Vector2Int>();
        visited = new HashSet<Vector2Int>();
        inFrontier = new HashSet<Vector2Int>();
        pathFound = false;

        nodeData[origin] = new DijkstraNodeData(0f, origin);
        frontier.Enqueue(origin, 0f);
        inFrontier.Add(origin);

        while (!IsComplete())
        {
            if (frontier.Count == 0)
                break;

            currentNode = frontier.Dequeue();
            inFrontier.Remove(currentNode);

            if (visited.Contains(currentNode))
                continue;
            visited.Add(currentNode);

            foreach (var neighbor in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
            {
                float newCost = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(neighbor.x, neighbor.y);

                // 第一次发现
                if (!nodeData.ContainsKey(neighbor))
                {
                    nodeData[neighbor] = new DijkstraNodeData(newCost, currentNode);
                    frontier.Enqueue(neighbor, newCost);
                    inFrontier.Add(neighbor);
                }
                // 找到更优路径，且仅当不在队列中时入队
                else if (newCost < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData(newCost, currentNode);
                    if (!inFrontier.Contains(neighbor))
                    {
                        frontier.Enqueue(neighbor, newCost);
                        inFrontier.Add(neighbor);
                    }
                }
            }

            float delay = Input.GetKey(KeyCode.Space) ? stepDelay * speedUpFactor : stepDelay;
            yield return new WaitForSeconds(delay);
        }

        if (visited.Contains(end))
        {
            pathFound = true;
            GenerateSolution();
            Debug.Log($"[Pathfinder] Generated solution ({solution.Count} nodes): " +string.Join(" → ", solution));
            Debug.Log("✅ Path found!");
        }
        else
        {
            Debug.LogWarning("❌ No path found.");
        }
    }
    public IEnumerator AStarSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {
        start = origin;
        end = destination;
        solution = new List<Vector2Int>();
        nodeData = new Dictionary<Vector2Int, DijkstraNodeData>();
        frontier = new PriorityQueue<Vector2Int>();
        visited = new HashSet<Vector2Int>();
        inFrontier = new HashSet<Vector2Int>();
        pathFound = false;

        nodeData[origin] = new DijkstraNodeData(0f, origin);
        frontier.Enqueue(origin, Heuristic(origin, end));
        inFrontier.Add(origin);

        while (!IsComplete())
        {
            if (frontier.Count == 0)
                break;

            currentNode = frontier.Dequeue();
            inFrontier.Remove(currentNode);

            if (visited.Contains(currentNode))
                continue;
            visited.Add(currentNode);

            if (currentNode == end)
            {
                pathFound = true;
                GenerateSolution();
                Debug.Log($"[A* Pathfinder] Solution ({solution.Count} nodes): " + string.Join(" → ", solution));
                yield break;
            }

            foreach (var neighbor in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
            {
                float newG = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(neighbor.x, neighbor.y);

                if (!nodeData.ContainsKey(neighbor) || newG < nodeData[neighbor].gCost)
                {
                    nodeData[neighbor] = new DijkstraNodeData(newG, currentNode);
                    float fScore = newG + Heuristic(neighbor, end);
                    if (!inFrontier.Contains(neighbor))
                    {
                        frontier.Enqueue(neighbor, fScore);
                        inFrontier.Add(neighbor);
                    }
                }
            }

            float delay = Input.GetKey(KeyCode.Space) ? stepDelay * speedUpFactor : stepDelay;
            yield return new WaitForSeconds(delay);
        }

        if (visited.Contains(end))
        {
            pathFound = true;
            GenerateSolution();
            Debug.Log("✅ A* Path found!");
        }
        else
        {
            Debug.LogWarning("❌ No A* path found.");
        }
    }
    private void GenerateSolution()
    {
        solution.Clear();
        var step = end;
        while (step != start)
        {
            solution.Add(step);
            step = nodeData[step].previous;
        }
        solution.Add(start);
        solution.Reverse();
    }

    
private bool IsComplete()
 {
     // 1) 达到迭代上限，强制退出
     if (currentIterations++ >= maxIterations)
     {
         Debug.LogWarning($"达到最大迭代次数 {maxIterations}，强制退出");
         return true;
     }

     // 2) 如果已经访问到终点，就完成
     if (visited.Contains(end))
         return true;

     // 3) 如果前沿队列空了，也完成（无路可走）
     if (frontier.Count == 0)
         return true;

     // 4) 否则继续搜索
     return false;
 }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
 {
     if (tilemap == null || nodeData == null)
         return;

     GUIStyle style = new GUIStyle { fontSize = 24, fontStyle = FontStyle.Bold };
     var startWS = tilemap.GetTileCenter(start.x, start.y);
     var endWS = tilemap.GetTileCenter(end.x, end.y);

     style.normal.textColor = Color.green;
     Handles.Label(startWS + Vector3.up * 0.6f, "START", style);
     DebugDrawing.DrawCircle(startWS, Quaternion.Euler(90, 0, 0), 0.8f, 8, Color.green, Time.deltaTime, false);

     style.normal.textColor = Color.yellow;
     Handles.Label(endWS + Vector3.up * 0.8f, "END", style);
     DebugDrawing.DrawCircle(endWS, Quaternion.Euler(90, 0, 0), 0.8f, 5, Color.red, Time.deltaTime, false);

     if (solution != null && solution.Count > 1)
     {
         Gizmos.color = Color.red;
         for (int i = 1; i < solution.Count; i++)
         {
             var a = tilemap.GetTileCenter(solution[i - 1].x, solution[i - 1].y);
             var b = tilemap.GetTileCenter(solution[i].x, solution[i].y);
             Gizmos.DrawLine(a, b);
         }
     }

     


        /*foreach (var kv in nodeData)
        {
            var pos = tilemap.GetTileCenter(kv.Key.x, kv.Key.y);
            Handles.Label(pos + Vector3.up * 0.4f, kv.Value.gCost.ToString("F0"), style);
        }*/
        if (showAllTileBaseCosts && tilemap != null)
        {
            var bounds = tilemap.GetBounds();
            style.normal.textColor = Color.blue;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    if (tilemap.IsTraversable(x, y))
                    {
                        var pos = tilemap.GetTileCenter(x, y);
                        float baseCost = tilemap.GetCostToEnterTile(x, y);
                        Handles.Label(pos + Vector3.up * 0.4f, baseCost.ToString("F0"), style);
                    }
                }
            }
        }

        // 2️⃣ 只显示走过路径的累计总代价
        if (showTileCosts && nodeData != null)
        {
            style.normal.textColor = Color.blue;

            foreach (var kv in nodeData)
            {
                var pos = tilemap.GetTileCenter(kv.Key.x, kv.Key.y);
                Handles.Label(pos + Vector3.up * 0.4f, kv.Value.gCost.ToString("F0"), style);
            }
        }


    }
#endif
}

