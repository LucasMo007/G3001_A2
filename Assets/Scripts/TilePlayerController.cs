using UnityEngine;

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

   
        if (!tilemapRef.IsTraversible(currentTilePos.x, currentTilePos.y))
        {
            currentTilePos = new Vector2Int(0, 0);
            if (!tilemapRef.IsTraversible(0, 0))
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

           
            if (tilemapRef.IsTraversible(nextTile.x, nextTile.y))
            {
                currentTilePos = nextTile;
                transform.position = tilemapRef.GetTileCenter(nextTile.x, nextTile.y);
                moveTimer = moveCooldown;
            }
        }
    }
}
