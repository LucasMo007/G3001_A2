
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

        // 1) Make a copy to avoid modifying the original solution
        var fullPath = new List<Vector2Int>(path);

        // 2) Skip the start tile, treat all subsequent tiles as steps
        //    (even if path.Count == 1, we will move to the target)
        List<Vector2Int> steps;
        if (fullPath.Count > 1 && fullPath[0] == currentTile)
            steps = fullPath.GetRange(1, fullPath.Count - 1);
        else
            steps = fullPath;

        if (steps.Count == 0)
            return;

        // 3) Start coroutine to move step by step through the tiles
        StopAllCoroutines();
        StartCoroutine(FollowPath(steps));
    }
}





