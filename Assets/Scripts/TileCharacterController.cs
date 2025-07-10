using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for any tile‐based character (Player, Monster, etc.).
/// Moves one tile at a time at a fixed speed, in cardinal directions only,
/// restricted by TilemapGameLevel's traversable tiles.
/// </summary>
[RequireComponent(typeof(Transform))]
public abstract class TileCharacterController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the TilemapGameLevel (must contain corridors & walls).")]
    public TilemapGameLevel tilemapRef;

    [Header("Movement Settings")]
    [Tooltip("Tiles moved per second (e.g. 5 = 0.2s per tile).")]
    public float tilesPerSecond = 5f;
    private float moveDuration => 1f / tilesPerSecond;

    // Current tile position (in grid coordinates)
    //protected Vector2Int currentTile;
    public Vector2Int currentTile { get; protected set; }
    // Whether the character is mid‐move
    protected bool isMoving;
    public bool IsMoving => isMoving;
    [Tooltip("Spawn point in tile coords (must be a floor tile).")]
    public Vector2Int startTile;


    protected virtual void Start()
    {
        // Auto‐find tilemap if not assigned
        if (tilemapRef == null)
            tilemapRef = Object.FindAnyObjectByType<TilemapGameLevel>();

        // Snap to nearest tile center
        Vector3Int cell = Vector3Int.FloorToInt(transform.position);
        currentTile = new Vector2Int(cell.x, cell.y);
        transform.position = tilemapRef.GetTileCenter(currentTile.x, currentTile.y);
        if (!tilemapRef.IsTraversable(currentTile.x, currentTile.y))
        {
            var size = tilemapRef.mapSizeTiles; // 比如是 (31,21)
            for (int x = 0; x < size.x; x++)
                for (int y = 0; y < size.y; y++)
                {
                    if (tilemapRef.IsTraversable(x, y))
                    {
                        currentTile = new Vector2Int(x, y);
                        transform.position = tilemapRef.GetTileCenter(x, y);
                        Debug.Log($"Snapped to floor at ({x},{y})");
                        return;
                    }
                }
        }
        
    }

   
    /// <param name="direction">Unit vector: up/down/left/right.</param>
    protected void TryMove(Vector2Int direction)
    {
        if (isMoving) return;
        var target = currentTile + direction;
        if (tilemapRef.IsTraversable(target.x, target.y))
            StartCoroutine(MoveToTile(target));
    }

   
    /// Moves the character over moveDuration seconds from currentTile to targetTile.
   
    private IEnumerator MoveToTile(Vector2Int targetTile)
    {
        isMoving = true;
        Vector3 start = transform.position;
        Vector3 end = tilemapRef.GetTileCenter(targetTile.x, targetTile.y);
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        currentTile = targetTile;
        isMoving = false;
    }

    /// <summary>
    /// Follows a precomputed path of tile coordinates.
    /// </summary>
    public IEnumerator FollowPath(List<Vector2Int> path)
    {
        foreach (var tile in path)
        {
            Debug.Log($"[Debug] FollowPath: 准备移动到 {tile}");
            yield return StartCoroutine(MoveToTile(tile));
            Debug.Log($"[Debug] FollowPath: 完成移动到 {tile}");
        }
    }
}




