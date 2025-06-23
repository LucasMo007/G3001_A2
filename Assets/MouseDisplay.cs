using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class MouseDisplay : MonoBehaviour
{
    private Tilemap map;
    public TextMeshProUGUI TextMeshGraphic;

    void Start()
    {
        map = FindAnyObjectByType<Tilemap>();
    }

    void Update()
    {
        Vector2 pixelPos = Input.mousePosition;
        Vector3 worldSpacePosition = Camera.main.ScreenToWorldPoint(pixelPos);
        worldSpacePosition.z = 0;

        int tileX = (int)worldSpacePosition.x;
        int tileY = (int)worldSpacePosition.y;
        Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);
        transform.position = map.GetCellCenterWorld(tilePos);

        TileBase tile = map.GetTile(tilePos);
        if (tile != null)
        {
            TextMeshGraphic.text = tile.name;
        }
        else
        {
            TextMeshGraphic.text = "empty";
        }

        TextMeshGraphic.text += "\n" + tileX + "," + tileY;

        // ✅ 右键 → 设置起点
        if (Input.GetMouseButtonDown(1))
        {
            FindAnyObjectByType<Pathfinder>().start = new Vector2Int(tileX, tileY);
        }

        // ✅ 左键 → 设置终点并开始寻路
        if (Input.GetMouseButtonDown(0))
        {
            if (tile != null)
            {
                var pathfinder = FindAnyObjectByType<Pathfinder>();
                pathfinder.end = new Vector2Int(tileX, tileY);
                pathfinder.FindPathDebugging();
            }
        }
    }
}