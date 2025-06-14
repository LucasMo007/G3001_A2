using TreeEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGameLevel : MonoBehaviour
{
    Tilemap map;
    [SerializeField] TileBase floorTile;
    public Vector2Int mapSizeTiles = new Vector2Int(10, 10);

    public float chanceToSpawnFloor = 0.75f;

    public float perlinScale = 0.1f;
    private void Start()
    {
        map = GetComponent<Tilemap>();
        //map.SetTile(new Vector3Int(0,0,0),floorTile);
        //map.SetTile(new Vector3Int(4,0,0),floorTile);
        GenerateMap();
    }
    public void SetChanceToSpawnFloor(float chance) 
    {
        chanceToSpawnFloor = chance;
    }
    public void GenerateMap()
    {
        //map.ClearAllTiles();
        for (int x = 0; x < mapSizeTiles.x; x++)
        {
            for (int y = 0; y < mapSizeTiles.y; y++)
            { Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (Mathf.PerlinNoise(x* perlinScale, y* perlinScale) < chanceToSpawnFloor)
                {

                    map.SetTile(tilePos, floorTile);

                }
                else
                {
                    map.SetTile(tilePos, null);
                }
            }
        }

    }
}