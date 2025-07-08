using UnityEngine;

/// <summary>
/// Example: Player controller deriving from TileCharacterController.
/// </summary>
public class PlayerCharacter : TileCharacterController
{
    protected override void Start()
    {
        base.Start();
    }

    void Update()
    {
      
        if (isMoving) return;

        // 根据按键调用基类的 TryMove
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
