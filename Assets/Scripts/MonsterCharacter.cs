using UnityEngine;
using System.Collections;
using System.Collections.Generic;


    public class MonsterCharacter : TileCharacterController
    {
    public void MoveAlongPath(List<Vector2Int> path)
    {
        if (path == null || path.Count == 0)
            return;
        Debug.Log($"[Debug] Full solution ({path.Count} nodes): " + string.Join(" → ", path));

        // 1) 复制一份，不要修改原来的 solution
        var fullPath = new List<Vector2Int>(path);

        // 2) 跳过起点，把后续所有格子都当作一步
        //    （即使 path.Count==1，也会走到 target）
        List<Vector2Int> steps;
        if (fullPath.Count > 1 && fullPath[0] == currentTile)
            steps = fullPath.GetRange(1, fullPath.Count - 1);
        else
            steps = fullPath;

        if (steps.Count == 0)
            return;

        // 3) 启动协程，按瓦块逐一移动
        StopAllCoroutines();
        StartCoroutine(FollowPath(steps));
    }
}




