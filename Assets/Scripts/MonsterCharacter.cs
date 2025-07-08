using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterCharacter : TileCharacterController
{
    /// <summary>
    /// 接收路径并沿路径移动
    /// </summary>
    public void MoveAlongPath(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return;
        path.RemoveAt(0);
        StopAllCoroutines();
        StartCoroutine(FollowPath(path));
    }
}



