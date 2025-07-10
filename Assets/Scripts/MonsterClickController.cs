
/*using UnityEngine;
using System.Collections.Generic;

public class MonsterClickController : MonoBehaviour
{
    public MonsterCharacter character;
    public GraphNavigator navigator;
    public TilemapGameLevel tilemap;
    [Tooltip("是否使用 A* 算法（否则使用 Dijkstra）")]
    public bool useAStar = false;

    // NEW: reference your Pathfinder (the one with DijkstraSearchCoroutine)
    public Pathfinder pathfinder;

    // NEW: are we currently stepping through?
    private bool isDebugging = false;

   /* private void Awake()
    {
        character = character ?? GetComponent<MonsterCharacter>();
        if (navigator == null) Debug.LogError("请在 Inspector 中拖入 GraphNavigator。");
        tilemap = tilemap ?? GetComponentInChildren<TilemapGameLevel>();
    }
    private void Awake()
    {
        character = character ?? GetComponent<MonsterCharacter>();
        navigator = navigator ?? GetComponent<GraphNavigator>();
        tilemap = tilemap ?? GetComponentInChildren<TilemapGameLevel>();

        // 获取 Pathfinder 组件
        if (pathfinder == null)
            pathfinder = GetComponent<Pathfinder>();

        if (navigator == null) Debug.LogError("请在 Inspector 中拖入 GraphNavigator。");
        if (pathfinder == null) Debug.LogError("请在 Inspector 中拖入 Pathfinder。");
    }
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var tgt = new Vector2Int(Mathf.FloorToInt(wp.x), Mathf.FloorToInt(wp.y));
        if (!tilemap.IsTraversable(tgt.x, tgt.y)) return;
        var path = navigator.FindTilePath(character.currentTile, tgt, useAStar);
        if (path == null || path.Count == 0) return;
        character.MoveAlongPath(path);
    }
    private void Update()
    {
        // ---- 1) 全局 Esc：优先处理，按下就退出调试 ----
        if (Input.GetKeyDown(KeyCode.Escape) && isDebugging)
        {
            isDebugging = false;
            pathfinder.StopAllCoroutines();
            Debug.Log("调试已取消，恢复正常模式。");
            // 不 return，让它直接继续到“非调试”流程，下一帧就能响应鼠标点击
        }

        // ---- 2) 如果当前不是调试状态，才允许发起新动作 ----
        if (!isDebugging)
        {
            // 2a) 右键：进入一步步调试模式
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int targetTile = new Vector2Int(
                    Mathf.FloorToInt(worldPos.x),
                    Mathf.FloorToInt(worldPos.y)
                );
                if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
                    return;

                // 清空旧的调试数据
              
                pathfinder.start = character.currentTile;
                pathfinder.end = targetTile;
                isDebugging = true;
                pathfinder.FindPathDebugging();
                Debug.Log($"[Debug] 开始路径调试：{pathfinder.start} → {pathfinder.end}");
                return;  // 本帧不再执行左键逻辑
            }

            // 2b) 左键：正常路径搜索并移动
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int targetTile = new Vector2Int(
                    Mathf.FloorToInt(worldPos.x),
                    Mathf.FloorToInt(worldPos.y)
                );
                if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
                    return;

                var path = navigator.FindTilePath(character.currentTile, targetTile, useAStar);
                if (path != null && path.Count > 0)
                {
                    Debug.Log($"[Move] {character.currentTile} → {targetTile}");
                    character.MoveAlongPath(path);
                }
                return;
            }
        }
        // ---- 3) 调试状态中，仅监听 Enter 来“确认”并移动 ----
        else
        {
            // 当 PathFound 为 true 且按下回车，退出调试并移动
            if (pathfinder.PathFound && Input.GetKeyDown(KeyCode.Return))
            {
                isDebugging = false;
                Debug.Log("[Debug] 路径已找到，开始正式移动。");
                character.MoveAlongPath(pathfinder.Solution);
            }
            // 其他输入都被忽略
        }
    }
}*/
using UnityEngine;
using System.Collections.Generic;

public class MonsterClickController : MonoBehaviour
{
    public MonsterCharacter character;
    public GraphNavigator navigator;
    public TilemapGameLevel tilemap;
    [Tooltip("是否使用 A* 算法（否则使用 Dijkstra）")]
    public bool useAStar = false;

    // NEW: reference your Pathfinder (the one with DijkstraSearchCoroutine)
    public Pathfinder pathfinder;

    // NEW: are we currently stepping through?
    private bool isDebugging = false;

    // 防止重复点击的冷却时间
    private float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.2f; // 200毫秒冷却

    private bool isSpeedingUp = false;
    private float lastSpacebarTime = 0f;
    private const float SPACEBAR_REPEAT_DELAY = 0.1f; // 按住空格键时的重复间隔


    private void Awake()
    {
        character = character ?? GetComponent<MonsterCharacter>();
        navigator = navigator ?? GetComponent<GraphNavigator>();
        tilemap = tilemap ?? GetComponentInChildren<TilemapGameLevel>();

        // 获取 Pathfinder 组件
        if (pathfinder == null)
            pathfinder = GetComponent<Pathfinder>();

        if (navigator == null) Debug.LogError("请在 Inspector 中拖入 GraphNavigator。");
        if (pathfinder == null) Debug.LogError("请在 Inspector 中拖入 Pathfinder。");
    }

    private void Update()
    {
        // ---- 1) 全局 Esc：优先处理，按下就退出调试 ----
        if (Input.GetKeyDown(KeyCode.Escape) && isDebugging)
        {
            CancelDebugging();
            return; // 立即返回，避免其他操作
        }

        // ---- 2) 如果当前不是调试状态，才允许发起新动作 ----
        if (!isDebugging)
        {
            // 2a) 右键：进入一步步调试模式
            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
                return;  // 本帧不再执行左键逻辑
            }

            // 2b) 左键：正常路径搜索并移动
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
                return;
            }
        }
        // ---- 3) 调试状态中，仅监听 Enter 来"确认"并移动 ----
        else
        {
            HandleSpacebarSpeedUp();
            // 当 PathFound 为 true 且按下回车，退出调试并移动
            if (pathfinder.PathFound && Input.GetKeyDown(KeyCode.Return))
            {
                ConfirmDebugPath();
            }
            // 其他输入都被忽略
        }
    }
    private void HandleSpacebarSpeedUp()
    {
        // 检查是否正在按住空格键
        if (Input.GetKey(KeyCode.Space))
        {
            // 如果刚开始按下空格键，立即触发一次
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerDebugStep();
                lastSpacebarTime = Time.time;
                isSpeedingUp = true;
                Debug.Log("[Debug] 开始加速步进...");
            }
            // 如果已经在按住状态，根据间隔时间重复触发
            else if (isSpeedingUp && Time.time - lastSpacebarTime >= SPACEBAR_REPEAT_DELAY)
            {
                TriggerDebugStep();
                lastSpacebarTime = Time.time;
            }
        }
        // 松开空格键
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isSpeedingUp)
            {
                isSpeedingUp = false;
                Debug.Log("[Debug] 停止加速步进");
            }
        }
    }

    private void TriggerDebugStep()
    {
        // 检查 pathfinder 是否有效
        if (pathfinder != null)
        {
            // 如果路径已找到，不再步进
            if (pathfinder.PathFound)
            {
                Debug.Log("[Debug] 路径已找到，无需继续步进");
                isSpeedingUp = false;
                return;
            }

            // 空格键加速实际上是通过修改 Pathfinder 内部的延迟来实现的
            // Pathfinder 的 DijkstraSearchCoroutine 会检查 Input.GetKey(KeyCode.Space)
            // 所以这里只需要确保协程正在运行即可
            Debug.Log("[Debug] 加速步进中...");
        }
    }

    private void HandleRightClick()
    {
        // 防止重复点击
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log("点击过于频繁，已忽略");
            return;
        }

        // 检查角色是否正在移动
        if (character.IsMoving)
        {
            Debug.Log("角色正在移动，无法开始调试");
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int targetTile = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
        {
            Debug.Log("目标位置不可通行");
            return;
        }

        // 检查起点和终点是否相同
        if (character.currentTile == targetTile)
        {
            Debug.Log("起点和终点相同，无需寻路");
            return;
        }

        // 安全启动调试
        StartDebugging(targetTile);
        lastClickTime = Time.time;
    }

    private void HandleLeftClick()
    {
        // 防止重复点击
        if (Time.time - lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log("点击过于频繁，已忽略");
            return;
        }

        // 检查角色是否正在移动
        if (character.IsMoving)
        {
            Debug.Log("角色正在移动，请等待完成");
            return;
        }

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int targetTile = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        if (!tilemap.IsTraversable(targetTile.x, targetTile.y))
        {
            Debug.Log("目标位置不可通行");
            return;
        }

        // 检查起点和终点是否相同
        if (character.currentTile == targetTile)
        {
            Debug.Log("已在目标位置");
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
            Debug.Log("无法找到路径");
        }

        lastClickTime = Time.time;
    }

    private void StartDebugging(Vector2Int targetTile)
    {
        try
        {
            // 确保清理之前的状态
            CancelDebugging();

            // 设置新的调试参数
            pathfinder.start = character.currentTile;
            pathfinder.end = targetTile;
            isDebugging = true;
            isSpeedingUp = false; // 重置加速状态
                                  // 启动调试寻路
            pathfinder.FindPathDebugging(useAStar);
            Debug.Log($"[Debug] 开始路径调试：{pathfinder.start} → {pathfinder.end}");
            Debug.Log($"[Debug] 提示：按住空格键可加速步进，Enter确认路径，Esc取消");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"启动调试失败: {e.Message}");
            CancelDebugging();
        }
    }

    private void CancelDebugging()
    {
        if (isDebugging)
        {
            isDebugging = false;
            isSpeedingUp = false; // 重置加速状态
            if (pathfinder != null)
            {
                pathfinder.StopAllCoroutines();
            }
            Debug.Log("调试已取消，恢复正常模式。");
        }
    }

    private void ConfirmDebugPath()
    {
        try
        {
            if (pathfinder.Solution != null && pathfinder.Solution.Count > 0)
            {
                isDebugging = false;
                isSpeedingUp = false; // 重置加速状态
                Debug.Log("[Debug] 路径已找到，开始正式移动。");
                character.MoveAlongPath(pathfinder.Solution);
            }
            else
            {
                Debug.LogWarning("没有有效的路径解决方案");
                CancelDebugging();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"确认路径失败: {e.Message}");
            CancelDebugging();
        }
    }

    // 公共方法：外部可以调用来取消调试
    public void ForceStopDebugging()
    {
        CancelDebugging();
    }

    // 检查当前状态的公共方法
    public bool IsCurrentlyDebugging => isDebugging;
    public bool IsSpeedingUp => isSpeedingUp;
    
}