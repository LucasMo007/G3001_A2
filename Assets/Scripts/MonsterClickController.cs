
using UnityEngine;
using System.Collections.Generic;

public class MonsterClickController : MonoBehaviour
{
    public MonsterCharacter character;
    public GraphNavigator navigator;
    public TilemapGameLevel tilemap;

    [Tooltip("Use A* Algorithm (otherwise Dijkstra)")]
    public bool useAStar = false;

    // Reference to your Pathfinder (the one with DijkstraSearchCoroutine)
    public Pathfinder pathfinder;

    // Are we currently in debugging (step-through) mode?
    private bool isDebugging = false;

    // Click cooldown to prevent double-click spamming
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.2f; // 200 ms cooldown

    private bool isSpeedingUp = false;
    private float lastSpacebarTime = 0f;
    private const float SPACEBAR_REPEAT_DELAY = 0.1f; // Repeat interval when holding spacebar


    private void Awake()
    {
        character = character ?? GetComponent<MonsterCharacter>();
        navigator = navigator ?? GetComponent<GraphNavigator>();
        tilemap = tilemap ?? GetComponentInChildren<TilemapGameLevel>();

        // Get Pathfinder component
        if (pathfinder == null)
            pathfinder = GetComponent<Pathfinder>();

        if (navigator == null) Debug.LogError("Please assign GraphNavigator in the Inspector.");
        if (pathfinder == null) Debug.LogError("Please assign Pathfinder in the Inspector.");
    }

    private void Update()
    {
        // ---- 1) Global Esc: highest priority, cancels debugging immediately ----
        if (Input.GetKeyDown(KeyCode.Escape) && isDebugging)
        {
            CancelDebugging();
            return;
        }

        // ---- 2) If not currently debugging, allow new actions ----
        if (!isDebugging)
        {
            // 2a) Right-click: enter step-through debugging mode
            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
                return;
            }

            // 2b) Left-click: normal pathfinding and movement
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
                return;
            }
        }
        // ---- 3) In debugging mode, only listen for Enter to "confirm" and move ----
        else
        {
            HandleSpacebarSpeedUp();
            if (pathfinder.PathFound && Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmDebugPath();
            }
            // All other input ignored while debugging
        }
    }

    private void HandleSpacebarSpeedUp()
    {
        // Check if spacebar is being held
        if (Input.GetKey(KeyCode.Space))
        {
            // If just pressed, trigger immediately
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerDebugStep();
                lastSpacebarTime = Time.time;
                isSpeedingUp = true;
                Debug.Log("[Debug] Started accelerated stepping...");
            }
            // If held down, repeat at interval
            else if (isSpeedingUp && Time.time - lastSpacebarTime >= SPACEBAR_REPEAT_DELAY)
            {
                TriggerDebugStep();
                lastSpacebarTime = Time.time;
            }
        }
        // Released spacebar
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isSpeedingUp)
            {
                isSpeedingUp = false;
                Debug.Log("[Debug] Stopped accelerated stepping");
            }
        }
    }

    private void TriggerDebugStep()
    {
        if (pathfinder != null)
        {
            if (pathfinder.PathFound)
            {
                Debug.Log("[Debug] Path already found, no further stepping needed");
                isSpeedingUp = false;
                return;
            }

            // Note: actual stepping is handled inside Pathfinder by checking Input.GetKey(KeyCode.Space)
            Debug.Log("[Debug] Accelerated stepping...");
        }
    }

    private void HandleRightClick()
    {
        // Prevent double-click spamming
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log("Click too frequent, ignored");
            return;
        }

        // Prevent starting debug while moving
        if (character.IsMoving)
        {
            Debug.Log("Character is moving, cannot start debugging");
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int targetTile = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
        {
            Debug.Log("Target location is not walkable");
            return;
        }

        if (character.currentTile == targetTile)
        {
            Debug.Log("Start and target tiles are the same, no pathfinding needed");
            return;
        }

        StartDebugging(targetTile);
        lastClickTime = Time.time;
    }

    private void HandleLeftClick()
    {
        // Prevent double-click spamming
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log("Click too frequent, ignored");
            return;
        }

        if (character.IsMoving)
        {
            Debug.Log("Character is moving, please wait until it finishes");
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int targetTile = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
        {
            Debug.Log("Target location is not walkable");
            return;
        }

        if (character.currentTile == targetTile)
        {
            Debug.Log("Already at target location");
            return;
        }

        var path = navigator.FindTilePath(character.currentTile, targetTile, useAStar);
        if (path != null && path.Count > 0)
        {
            Debug.Log($"[Move] {character.currentTile} → {targetTile}");
            character.MoveAlongPath(path);
        }
        else
        {
            Debug.Log("No path found");
        }

        lastClickTime = Time.time;
    }

    private void StartDebugging(Vector2Int targetTile)
    {
        try
        {
            CancelDebugging();

            pathfinder.start = character.currentTile;
            pathfinder.end = targetTile;
            isDebugging = true;
            isSpeedingUp = false; // Reset acceleration state

            pathfinder.FindPathDebugging(useAStar);

            Debug.Log($"[Debug] Started path debugging: {pathfinder.start} → {pathfinder.end}");
            Debug.Log($"[Debug] Tip: Hold spacebar to step quickly, Enter to confirm path, Esc to cancel");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start debugging: {e.Message}");
            CancelDebugging();
        }
    }

    private void CancelDebugging()
    {
        if (isDebugging)
        {
            isDebugging = false;
            isSpeedingUp = false;
            if (pathfinder != null)
            {
                pathfinder.StopAllCoroutines();
            }
            Debug.Log("Debugging cancelled, returning to normal mode.");
        }
    }

    private void ConfirmDebugPath()
    {
        try
        {
            if (pathfinder.Solution != null && pathfinder.Solution.Count > 0)
            {
                isDebugging = false;
                isSpeedingUp = false;
                Debug.Log("[Debug] Path confirmed, starting actual movement.");
                character.MoveAlongPath(pathfinder.Solution);
            }
            else
            {
                Debug.LogWarning("No valid path solution found");
                CancelDebugging();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to confirm path: {e.Message}");
            CancelDebugging();
        }
    }

    // Public method to cancel debugging from outside
    public void ForceStopDebugging()
    {
        CancelDebugging();
    }

    // Public checkers for external UI or logic
    public bool IsCurrentlyDebugging => isDebugging;
    public bool IsSpeedingUp => isSpeedingUp;
}
