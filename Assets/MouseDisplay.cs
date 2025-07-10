
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
        // Get mouse position in world
        Vector2 pixelPos = Input.mousePosition;
        Vector3 worldSpacePosition = Camera.main.ScreenToWorldPoint(pixelPos);
        worldSpacePosition.z = 0;

        // Convert world to tile grid position
        int tileX = (int)worldSpacePosition.x;
        int tileY = (int)worldSpacePosition.y;
        Vector3Int tilePos = new Vector3Int(tileX, tileY, 0);

        // Move this GameObject to the tile center
        transform.position = map.GetCellCenterWorld(tilePos);

        // Get the tile at that position
        TileBase tile = map.GetTile(tilePos);

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

        // ✅ Removed all mouse click bindings!
    }
}
