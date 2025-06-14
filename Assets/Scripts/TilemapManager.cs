using UnityEngine;
using UnityEngine.Tilemaps;



public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap; // 拖拽引用场景中的 Tilemap
    public int width = 10;
    public int height = 10;

    private TileBase floorTile;
    private TileBase nullTile;

    private void Start()
    {
        // 从 Resources 文件夹加载 Tile 资源
        floorTile = Resources.Load<TileBase>("Tiles/floorTile");
        nullTile = Resources.Load<TileBase>("Tiles/NullTile");

        if (floorTile == null || nullTile == null)
        {
            Debug.LogError("❌ 无法加载 FloorTile 或 NullTile，请检查 Resources/Tiles/ 目录！");
            return;
        }

        GenerateTestLevel(); // 程序化生成地图（初始测试）
    }

    /// <summary>
    /// 简单生成一个中间是 Floor，周围是 Null 的地图
    /// </summary>
    private void GenerateTestLevel()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // 设置边缘为 NullTile，内部为 FloorTile
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    tilemap.SetTile(tilePos, nullTile);
                }
                else
                {
                    tilemap.SetTile(tilePos, floorTile);
                }
            }
        }

        Debug.Log("✅ 测试地图生成完成！");
    }

    /// <summary>
    /// 判断某个 tile 是否为 FloorTile（可通行）
    /// </summary>
    public bool IsWalkable(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        return tile == floorTile;
    }

    /// <summary>
    /// 设置某个位置为 Floor 或 Null
    /// </summary>
    public void SetTile(Vector3Int position, bool walkable)
    {
        tilemap.SetTile(position, walkable ? floorTile : nullTile);
    }
}
