using UnityEngine;
using UnityEngine.Tilemaps;



public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap; 
    public int width = 10;
    public int height = 10;

    private TileBase floorTile;
    private TileBase nullTile;

    private void Start()
    {
      
        floorTile = Resources.Load<TileBase>("Tiles/floorTile");
        nullTile = Resources.Load<TileBase>("Tiles/NullTile");

        if (floorTile == null || nullTile == null)
        {
           
            return;
        }

        GenerateTestLevel();
    }

 
    private void GenerateTestLevel()
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

              
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

     
    }

    
    public bool IsWalkable(Vector3Int position)
    {
        TileBase tile = tilemap.GetTile(position);
        return tile == floorTile;
    }

   
    public void SetTile(Vector3Int position, bool walkable)
    {
        tilemap.SetTile(position, walkable ? floorTile : nullTile);
    }
}
