
using UnityEngine;
using System.Collections.Generic;

public class MonsterClickController : MonoBehaviour
{
    public MonsterCharacter character;
    public GraphNavigator navigator;
    public TilemapGameLevel tilemap;
    [Tooltip("是否使用 A* 算法（否则使用 Dijkstra）")]
    public bool useAStar = false;

    private void Awake()
    {
        character = character ?? GetComponent<MonsterCharacter>();
        if (navigator == null) Debug.LogError("请在 Inspector 中拖入 GraphNavigator。");
        tilemap = tilemap ?? GetComponentInChildren<TilemapGameLevel>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var tgt = new Vector2Int(Mathf.FloorToInt(wp.x), Mathf.FloorToInt(wp.y));
        if (!tilemap.IsTraversable(tgt.x, tgt.y)) return;
        var path = navigator.FindTilePath(character.currentTile, tgt, useAStar);
        if (path == null || path.Count == 0) return;
        character.MoveAlongPath(path);
    }
}