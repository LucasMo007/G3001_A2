using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using static UnityEditor.Progress;

public class Pathfinder : MonoBehaviour
{
    private Dictionary<Vector2Int, DijkstraNodeData> nodeData;//save all node coordiante and cost

    // store information about each node during Dijkstra’s pathfinding.
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
    [SerializeField] private Vector2Int startCoord = new Vector2Int(0, 0);//the startcoord is start（0，0）,all the start's coordinate is (0,0).
    [SerializeField] private Vector2Int endCoord = new Vector2Int(5, 5);//endCoord is end (5,5)

    private Vector2Int currentNode;

    // Core algorithm data
    private PriorityQueue<Vector2Int> frontier;
    private HashSet<Vector2Int> visited;

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

    public float GetTotalCostToReach(Vector2Int node)
    {
        if (nodeData.ContainsKey(node))
            return nodeData[node].gCost;
        return float.PositiveInfinity;
    }

    public void UpdateBestWayToReachTile(Vector2Int origin, Vector2Int destination, float cost)
    {
        nodeData[destination] = new DijkstraNodeData(cost, origin);
    }
    //Discovered = Visited + Frontier
    public bool IsDiscovered(Vector2Int node)
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
    //6.Get Lowest Cost Node from the Unvisited Set
    public Tuple<Vector2Int, float> GetLowestCostInUnvisited()
    {
        Vector2Int bestNode = new Vector2Int(int.MaxValue, int.MaxValue);
        float bestCost = float.PositiveInfinity;

        foreach (Vector2Int node in nodeData.Keys)
        {
            if (!visited.Contains(node))
            {
                float cost = nodeData[node].gCost;
                if (cost < bestCost)//cost is current note cost ,cost = previous 的 gCost + 这一步要付的代价
                {                    // bestCost is 最小的代价，save in visited list 
                    bestCost = cost;
                    bestNode = node;
                }
            }
        }

        return new Tuple<Vector2Int, float>(bestNode, bestCost);
    }

    public void DijkstraIteration()//9.Prototype Dijkstra’s Algorithm’s main loop
    {
        currentNode = GetLowestCostInUnvisited().Item1;

        Debug.Log("Visiting: " + currentNode + ", cost: " + nodeData[currentNode].gCost);
        DebugDrawing.DrawCircle(tilemap.GetTileCenter(currentNode.x, currentNode.y), Quaternion.AngleAxis(90, Vector3.forward), 0.6f, 16, Color.yellow, 0.1f, false);
        //Relation step
        foreach (Vector2Int connected in tilemap.GetAdjacentTiles(currentNode.x, currentNode.y))
        {
            float costToReachConnected = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(connected.x, connected.y);//current cost = previous + cost (previous->current)
            //  costToReachConnected is the total cost of neighbor ,nodeData[currentNode].gCost is total cost of current visited node ,
            //  tilemap.GetCostToEnterTile(connected.x, connected.y) is cost from current to neighboor
            if (!IsDiscovered(connected))//connect is neighbor node 
            {
                Debug.Log("Discovered: " + connected + ", cost: " + costToReachConnected);
                UpdateBestWayToReachTile(currentNode, connected, costToReachConnected);
                frontier.Enqueue(connected, costToReachConnected);
            }
            else if (costToReachConnected < nodeData[connected].gCost)
            {
                UpdateBestWayToReachTile(currentNode, connected, costToReachConnected);
                //Enqueue do?
                //Adds an item to the priority queue.
                frontier.Enqueue(connected, costToReachConnected);//frontier is usually a priority queue used in pathfinding algorithms like Dijkstra’s.
            }
        }

        MoveToVisitedSet(currentNode);
    }

    public IEnumerator StepThroughDijkstra(float delaySeconds = 0.5f)
    {
        while (!IsComplete())//While the algorithm is not yet finished 
        {
            DijkstraIteration();//Call DijkstraIteration(), which performs one step of the algorithm (e.g. visiting one node, expanding neighbors).
            yield return new WaitForSeconds(delaySeconds);//Wait for delaySeconds before continuing (default is 0.5 seconds).
        }

        if (IsSolved())//After the loop finishes (the algorithm is "complete"):
        {
            ReconstructPath();//call ReconstructPath() to generate the final path list.
            Debug.Log("✅ Path found!");
        }
        else
        {
            Debug.LogWarning("❌ No path found.");
        }
    }
    //Once the search is done, this method builds the final path by tracing back from end to start using the previous pointers you stored during the search.
    private void ReconstructPath()
    {
        solution.Clear();//Clears any old solution path.
        Vector2Int step = end;//Start backtracking from the end node.

        while (step != start)//Stops when you reach the start node.
        {
            solution.Add(step);//Add the current step to the solution list.
            step = nodeData[step].previous;//Move to the previous node recorded during search.
        }

        solution.Add(start);//Don’t forget to add the start node itself.
        solution.Reverse();//Don’t forget to add the start node itself.
        pathFound = true;//Sets a flag indicating that a valid path was found.
    }

    private bool IsSolved()//if the end is visited 
    {
        return IsVisited(end);
    }

    private bool IsComplete()//if the  end is found and there is no way 
    {
        return IsSolved() || GetLowestCostInUnvisited().Item2 == float.PositiveInfinity;
    }

    void GenerateSolution()//save all coordinate of visited node from the end to the start 
    {
        if (!IsSolved())
        {
            throw new Exception("Not solved! Cannot generate solution");
        }

        solution = new List<Vector2Int>();

        Vector2Int currentNode = end;
        do
        {
            solution.Add(currentNode);
            currentNode = nodeData[currentNode].previous;
        } while (currentNode != start);

        solution.Add(start);
        solution.Reverse();
    }

    public IEnumerator DijkstraSearchCoroutine(Vector2Int origin, Vector2Int destination)
    {   //7. Set Initial Conditions
        start = origin;
        end = destination;
        solution = new List<Vector2Int>();
        nodeData = new Dictionary<Vector2Int, DijkstraNodeData>();
        frontier = new PriorityQueue<Vector2Int>();
        visited = new HashSet<Vector2Int>();
        pathFound = false;

        nodeData[origin] = new DijkstraNodeData(0f, origin);//cost of start=0,coordiate of (0,0),the cost is 0
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
                float newCost = nodeData[currentNode].gCost + tilemap.GetCostToEnterTile(neighbor.x, neighbor.y);//tilemap.GetCostToEnterTile(neighbor.x, neighbor.y)==1

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
            pathFound = true;
            Debug.Log("✅ Path found!");
            GenerateSolution();
        }
        else
        {
            Debug.LogWarning("❌ No path found.");
        }
    }
    //13. Generate a solution
    void OnDrawGizmos()
    {  //If there is no tilemap or no nodeData, just exit immediately.
        if (tilemap == null || nodeData == null)
            return;
        //Create a GUI style for text labels in the scene.
        //Sets the font size to 24 and makes it bold.
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        //Converts grid (tile) coordinates for start and end into world space coordinates.
        Vector3 startNodeWorldspace = tilemap.GetTileCenter(start.x, start.y);
        Vector3 endNodeWorldspace = tilemap.GetTileCenter(end.x, end.y);

        style.normal.textColor = new Color(0.05f, 0.8f, 0.05f, 1.0f);//Sets label text color to green.
        Handles.Label(startNodeWorldspace + Vector3.up * 0.4f, "START", style);//Draws the label "START" slightly above the start node.
        //Draws a green circle at the start position to highlight it.
        DebugDrawing.DrawCircle(startNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 8, Color.green, Time.deltaTime, false);

        style.normal.textColor = new Color(0.8f, 0.05f, 0.05f, 1.0f);//Sets label text color to red.
        Handles.Label(endNodeWorldspace + Vector3.up * 0.4f, "END", style);//Draws the label "END" above the end node.
        //Draws a red circle at the end position.
        DebugDrawing.DrawCircle(endNodeWorldspace, Quaternion.AngleAxis(90, Vector3.forward), 0.8f, 5, Color.red, Time.deltaTime, false);

        if (nodeData != null)
        {
            if (solution != null && solution.Count > 1)//If a solution path has been found (solution list with more than one node):
                Gizmos.color = Color. red;//Sets Gizmo color to cyan.
            for (int i = 1; i < solution.Count; i++)
                {
                    Vector3 from = tilemap.GetTileCenter(solution[i - 1].x, solution[i - 1].y);
                    Vector3 to = tilemap.GetTileCenter(solution[i].x, solution[i].y);
                    Gizmos.DrawLine(from, to);//Draws lines connecting the nodes in the solution path in order.
            }
        }

            style.normal.textColor = new Color(0.05f, 0.05f, 0.05f, 1.0f);//Sets text color for cost labels to dark gray.

        foreach (KeyValuePair<Vector2Int, DijkstraNodeData> pair in nodeData)
        {     //Gets each node’s position, previous node, and cost.

                 Vector2Int nodePos = pair.Key;
                Vector2Int prev = pair.Value.previous;
                float cost = pair.Value.gCost;
            //Converts their tile positions to world space.
                Vector3 nodePosWorldspace = tilemap.GetTileCenter(nodePos.x, nodePos.y);
                Vector3 prevNodePosWorldspace = tilemap.GetTileCenter(prev.x, prev.y);
            //Draws a line from the previous node to this node.
            //The line color uses HSV coloring based on cost (e.g., higher cost = different color).
            //Debug.DrawLine(prevNodePosWorldspace, nodePosWorldspace, Color.HSVToRGB(cost / 10, 1, 0.8f), Time.deltaTime);
            //Draws a label showing the cost value above each node.
            Handles.Label(nodePosWorldspace + Vector3.up * 0.4f, cost.ToString("F0"), style);
            }
    }
}


