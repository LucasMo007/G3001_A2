using UnityEngine;

public class TilePlayerController : MonoBehaviour
{
    public TilemapGameLevel tilemapRef; // 拖拽 TilemapGameLevel 脚本对象
    public float moveCooldown = 0.15f; // 每次移动间隔
    private float moveTimer = 0f;

    private Vector2Int currentTilePos;

    /*void Start()
    {
        // 初始化当前位置为角色初始世界坐标转 tile 坐标
        Vector3Int cell = Vector3Int.FloorToInt(transform.position);
        currentTilePos = new Vector2Int(cell.x, cell.y);

        // 保证角色从合法 tile 开始
        if (!tilemapRef.IsTraversible(currentTilePos.x, currentTilePos.y))
        {
            Debug.LogWarning("Player not starting on a traversible tile!");
        }

        // 将角色中心对齐 tile 中心
        transform.position = tilemapRef.GetTileCenter(currentTilePos.x, currentTilePos.y);
    }*/
    void Start()
    {
        Vector3Int cell = Vector3Int.FloorToInt(transform.position);
        currentTilePos = new Vector2Int(cell.x, cell.y);

        // 如果当前位置不是合法 tile，尝试移动到 (0,0)
        if (!tilemapRef.IsTraversible(currentTilePos.x, currentTilePos.y))
        {
            currentTilePos = new Vector2Int(0, 0);
            if (!tilemapRef.IsTraversible(0, 0))
            {
                Debug.LogWarning("⚠️ Tile at (0,0) is also not walkable!");
                return;
            }
        }

        // 将角色对齐到 tile 中心
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

            // 判断下一格是否是可通行的 tile
            if (tilemapRef.IsTraversible(nextTile.x, nextTile.y))
            {
                currentTilePos = nextTile;
                transform.position = tilemapRef.GetTileCenter(nextTile.x, nextTile.y);
                moveTimer = moveCooldown;
            }
        }
    }
}
