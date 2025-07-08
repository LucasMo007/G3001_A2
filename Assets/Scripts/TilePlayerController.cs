/*using UnityEngine;

public class TilePlayerController : MonoBehaviour
{  //10. Create a Character
    public TilemapGameLevel tilemapRef; 
    public float moveCooldown = 0.15f; 
    private float moveTimer = 0f;

    private Vector2Int currentTilePos;

    
    void Start()
    {
        Vector3Int cell = Vector3Int.FloorToInt(transform.position);
        currentTilePos = new Vector2Int(cell.x, cell.y);

   
        if (!tilemapRef.IsTraversable(currentTilePos.x, currentTilePos.y))
        {
            currentTilePos = new Vector2Int(0, 0);
            if (!tilemapRef.IsTraversable(0, 0))
            {
                Debug.LogWarning(" Tile at (0,0) is also not walkable!");
                return;
            }
        }

    
        transform.position = tilemapRef.GetTileCenter(currentTilePos.x, currentTilePos.y);
    }
    void Update()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer > 0) return;

        Vector2Int input = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            input = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            input = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            input = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            input = Vector2Int.right;

        if (input != Vector2Int.zero)
        {
            Vector2Int nextTile = currentTilePos + input;

           
            if (tilemapRef.IsTraversable(nextTile.x, nextTile.y))
            {
                currentTilePos = nextTile;
                transform.position = tilemapRef.GetTileCenter(nextTile.x, nextTile.y);
                moveTimer = moveCooldown;
            }
        }
    }
}*/
using UnityEngine;

/// <summary>
/// Player controller that uses TileCharacterController for
/// fixed‐speed, tile‐to‐tile movement in 4 directions.
/// </summary>
public class TilePlayerController : TileCharacterController
{
    // 这里不再需要 tilemapRef、moveCooldown、moveTimer、currentTilePos，
    // 都由基类 TileCharacterController 管理。

    private void Update()
    {
        // 如果正在移动，就不接受新输入
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
