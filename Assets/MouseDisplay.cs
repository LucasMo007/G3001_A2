using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class MouseDisplay : MonoBehaviour
{
    private Tilemap map;
    public TextMeshProUGUI TextMeshGraphic;

    void Start()
    {
        map = FindAnyObjectByType<Tilemap>();//When the scene starts, it finds any Tilemap in the scene and stores it in map.
    }

    void Update()
    {    //Get mouse position in world
        Vector2 pixelPos = Input.mousePosition;
        Vector3 worldSpacePosition = Camera.main.ScreenToWorldPoint(pixelPos);
        worldSpacePosition.z = 0;
        // Convert world to tile grid position
        int tileX = (int)worldSpacePosition.x;
        int tileY = (int)worldSpacePosition.y;
        Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);

        //Move this GameObject to the tile center
        transform.position = map.GetCellCenterWorld(tilePos);

        TileBase tile = map.GetTile(tilePos);//Get the tile at that position

        // Update the TextMeshPro UI
        if (tile != null)
        {
            TextMeshGraphic.text = tile.name;
        }
        else
        {
            TextMeshGraphic.text = "empty";
        }

        TextMeshGraphic.text += "\n" + tileX + "," + tileY;

        //  right mouse button Sets Pathfinder.start to this tile.
        if (Input.GetMouseButtonDown(1))
        {
            FindAnyObjectByType<Pathfinder>().start = new Vector2Int(tileX, tileY);
        }

        // left mouse button Sets Pathfinder.end to this tile.
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