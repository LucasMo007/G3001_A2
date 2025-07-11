
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General-purpose Graph data structure supporting adding/removing vertices and edges,
/// as well as Dijkstra's shortest path algorithm.
/// Placed in the global namespace for seamless integration with other project classes (e.g. TilemapGameLevel).
/// </summary>

#region Vertex and Edge Definitions

// Vertex class
public class Vertex<T>
{
    public T Id { get; }
    public List<Edge<T>> Edges { get; } = new List<Edge<T>>();

    public Vertex(T id)
    {
        Id = id;
    }

    public void AddEdge(Edge<T> edge)
    {
        if (!Edges.Contains(edge))
            Edges.Add(edge);
    }

    public void RemoveEdge(Edge<T> edge)
    {
        Edges.Remove(edge);
    }

    public override string ToString() => Id.ToString();
}

// Edge class
public class Edge<T>
{
    public Vertex<T> From { get; }
    public Vertex<T> To { get; }
    public float Weight { get; }

    public Edge(Vertex<T> from, Vertex<T> to, float weight = 1f)
    {
        From = from;
        To = to;
        Weight = weight;
    }

    public override string ToString() => $"{From} -> {To} (Weight={Weight})";
}

#endregion

#region Graph Structure and Operations
public class Graph<T>
{
    private Dictionary<T, Vertex<T>> vertices = new Dictionary<T, Vertex<T>>();

    /// <summary>Adds a vertex; returns existing instance if it already exists.</summary>
    public Vertex<T> AddVertex(T id)
    {
        if (!vertices.TryGetValue(id, out var v))
        {
            v = new Vertex<T>(id);
            vertices[id] = v;
        }
        return v;
    }

    /// <summary>Removes the specified vertex and all its associated edges.</summary>
    public bool RemoveVertex(T id)
    {
        if (!vertices.TryGetValue(id, out var v)) return false;

        // Remove all edges pointing to this vertex
        foreach (var u in vertices.Values)
        {
            u.Edges.RemoveAll(e => e.To.Id.Equals(id));
        }

        return vertices.Remove(id);
    }

    /// <summary>Adds an edge; directed=true for directed edges, otherwise undirected.</summary>
    public void AddEdge(T fromId, T toId, float weight = 1f, bool directed = true)
    {
        var fromV = AddVertex(fromId);
        var toV = AddVertex(toId);
        var edge = new Edge<T>(fromV, toV, weight);
        fromV.AddEdge(edge);

        if (!directed)
        {
            var rev = new Edge<T>(toV, fromV, weight);
            toV.AddEdge(rev);
        }
    }

    /// <summary>Removes an edge; directed=true for one-way removal, otherwise both directions.</summary>
    public void RemoveEdge(T fromId, T toId, bool directed = true)
    {
        var fromV = GetVertex(fromId);
        var toV = GetVertex(toId);
        if (fromV == null || toV == null) return;

        fromV.RemoveEdge(fromV.Edges.Find(e => e.To.Id.Equals(toId)));
        if (!directed)
            toV.RemoveEdge(toV.Edges.Find(e => e.To.Id.Equals(fromId)));
    }

    /// <summary>Retrieves the vertex instance with the given ID.</summary>
    public Vertex<T> GetVertex(T id)
    {
        vertices.TryGetValue(id, out var v);
        return v;
    }

    /// <summary>All vertices in the graph.</summary>
    public IEnumerable<Vertex<T>> Vertices => vertices.Values;

    /// <summary>Gets neighbors of a vertex and their edge weights.</summary>
    public IEnumerable<(T neighbor, float weight)> GetNeighbors(T id)
    {
        var v = GetVertex(id);
        if (v == null) yield break;

        foreach (var e in v.Edges)
        {
            yield return (e.To.Id, e.Weight);
        }
    }

    /// <summary>All edges in the graph.</summary>
    public IEnumerable<Edge<T>> Edges
    {
        get
        {
            foreach (var v in vertices.Values)
                foreach (var e in v.Edges)
                    yield return e;
        }
    }

    #endregion

    #region Dijkstra's Shortest Path Algorithm

    /// <summary>
    /// Computes the shortest path from start to end as a list of node IDs.
    /// Returns an empty list if no path exists.
    /// Depends on the project's PriorityQueue&lt;T&gt;.
    /// </summary>
    public List<T> Dijkstra(T startId, T endId)
    {
        var dist = new Dictionary<T, float>();
        var prev = new Dictionary<T, T>();
        var pq = new PriorityQueue<T>();

        // Initialize distances
        foreach (var v in vertices.Keys)
            dist[v] = float.PositiveInfinity;
        if (!dist.ContainsKey(startId) || !dist.ContainsKey(endId))
            return new List<T>();

        dist[startId] = 0f;
        pq.Enqueue(startId, 0f);

        while (pq.Count > 0)
        {
            var u = pq.Dequeue();
            if (u.Equals(endId)) break;

            foreach (var (nbr, wt) in GetNeighbors(u))
            {
                float alt = dist[u] + wt;
                if (alt < dist[nbr])
                {
                    dist[nbr] = alt;
                    prev[nbr] = u;
                    pq.Enqueue(nbr, alt);
                }
            }
        }

        // Reconstruct path
        var path = new List<T>();
        T cur = endId;
        if (!prev.ContainsKey(cur) && !cur.Equals(startId))
            return path;

        while (true)
        {
            path.Add(cur);
            if (cur.Equals(startId)) break;
            cur = prev[cur];
        }
        path.Reverse();
        return path;
    }

    #endregion
}

#region Tilemap to Graph Extension

public static class GraphExtensions
{
    /// <summary>
    /// Converts a TilemapGameLevel to a Graph&lt;Vector2Int&gt;, automatically adding all traversable tiles and their edges (4-directional).
    /// </summary>
    public static Graph<Vector2Int> ToGraph(this TilemapGameLevel level)
    {
        var graph = new Graph<Vector2Int>();
        var bounds = level.GetBounds();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (!level.IsTraversable(x, y)) continue;

                var pos = new Vector2Int(x, y);
                graph.AddVertex(pos);

                foreach (var n in level.GetAdjacentTiles(x, y))
                {
                    float cost = level.GetCostToEnterTile(n.x, n.y);
                    graph.AddEdge(pos, n, cost, directed: false);
                }
            }
        }

        return graph;
    }
}
#endregion
