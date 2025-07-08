using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用 Graph 数据结构，支持顶点/边的添加、移除，以及 Dijkstra 最短路径算法。
/// 置于全局命名空间，以便与项目中其他类（如 TilemapGameLevel）无缝集成。
/// </summary>

#region 顶点与边定义
// 顶点类
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

// 边类
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

#region 图结构与操作
public class Graph<T>
{
    private Dictionary<T, Vertex<T>> vertices = new Dictionary<T, Vertex<T>>();

    /// <summary>添加顶点，若存在则返回已存在实例。</summary>
    public Vertex<T> AddVertex(T id)
    {
        if (!vertices.TryGetValue(id, out var v))
        {
            v = new Vertex<T>(id);
            vertices[id] = v;
        }
        return v;
    }

    /// <summary>移除指定顶点及其所有关联边。</summary>
    public bool RemoveVertex(T id)
    {
        if (!vertices.TryGetValue(id, out var v)) return false;
        // 移除所有指向此顶点的边
        foreach (var u in vertices.Values)
        {
            u.Edges.RemoveAll(e => e.To.Id.Equals(id));
        }
        return vertices.Remove(id);
    }

    /// <summary>添加边，directed=true 为有向，否则双向。</summary>
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

    /// <summary>移除边，directed=true 为单向移除，否则双向。</summary>
    public void RemoveEdge(T fromId, T toId, bool directed = true)
    {
        var fromV = GetVertex(fromId);
        var toV = GetVertex(toId);
        if (fromV == null || toV == null) return;
        fromV.RemoveEdge(fromV.Edges.Find(e => e.To.Id.Equals(toId)));
        if (!directed)
            toV.RemoveEdge(toV.Edges.Find(e => e.To.Id.Equals(fromId)));
    }

    /// <summary>获取顶点实例。</summary>
    public Vertex<T> GetVertex(T id)
    {
        vertices.TryGetValue(id, out var v);
        return v;
    }

    /// <summary>所有顶点集合。</summary>
    public IEnumerable<Vertex<T>> Vertices => vertices.Values;

    /// <summary>获取某顶点邻居及对应边权重。</summary>
    public IEnumerable<(T neighbor, float weight)> GetNeighbors(T id)
    {
        var v = GetVertex(id);
        if (v == null) yield break;
        foreach (var e in v.Edges)
        {
            yield return (e.To.Id, e.Weight);
        }
    }

    /// <summary>所有边集合。</summary>
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

    #region Dijkstra 最短路径算法
    /// <summary>
    /// 计算从 start 到 end 的最短路径（节点 ID 列表），若无路径则返回空列表。
    /// 依赖项目中 PriorityQueue&lt;T&gt;。
    /// </summary>
    public List<T> Dijkstra(T startId, T endId)
    {
        var dist = new Dictionary<T, float>();
        var prev = new Dictionary<T, T>();
        var pq = new PriorityQueue<T>();

        // 初始化
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

        // 重建路径
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

#region Tilemap 转 Graph 扩展
public static class GraphExtensions
{
    /// <summary>
    /// 将 TilemapGameLevel 转为 Graph&lt;Vector2Int&gt;，自动添加所有可通行瓦片与边（4 方向）。
    /// </summary>
    public static Graph<Vector2Int> ToGraph(this TilemapGameLevel level)
    {
        var graph = new Graph<Vector2Int>();
        var bounds = level.GetBounds();
        for (int x = bounds.xMin; x < bounds.xMax; x++)
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
        return graph;
    }
}
#endregion