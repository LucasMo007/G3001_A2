
using UnityEngine;

/// <summary>
/// Player controller that uses TileCharacterController for
/// fixed‐speed, tile‐to‐tile movement in 4 directions.
/// </summary>
public class TilePlayerController : TileCharacterController
{
    
    private void Update()
    {
      
        if (isMoving) return;

     
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            TryMove(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            TryMove(Vector2Int.down);
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            TryMove(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            TryMove(Vector2Int.right);
    }
}
