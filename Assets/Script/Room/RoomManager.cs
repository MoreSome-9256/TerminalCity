using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("房间数据库（ScriptableObject）")]
    public RoomDatabase database;

    [Header("本区域包含的房间 ID 列表")]
    public List<int> roomIDsInArea = new List<int>();

    // 存储运行时房间数据
    private Dictionary<int, RoomState> roomStates = new Dictionary<int, RoomState>();
    //private Dictionary<int, Room> roomDict = new Dictionary<int, Room>();

    public float AreaInstability { get; private set; }

    private GameObject currentRoom;
    public int currentRoomID = -1;

    private float chaosTickTimer = 0f;
    // 记录走廊上次视角位置
    private Dictionary<int, Vector2> lastCorridorViewPositions = new Dictionary<int, Vector2>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(this);

        // 初始化区域内所有房间的运行时状态
        /*foreach (int id in roomIDsInArea)
        {
            if (!roomStates.ContainsKey(id))
            {
                roomStates[id] = new RoomState(id, 9500, 10000); 
            }
        }*/
        foreach (int id in roomIDsInArea)
        {
            if (!roomStates.ContainsKey(id))
            {
                int defaultInstability = 9500;
                int threshold = 10000;

                // 你可以在这里做差异化初始化
                if (id == 11) defaultInstability = 9700;
                if (id == 2) defaultInstability = 9900;

                roomStates[id] = new RoomState(id, defaultInstability, threshold);
            }
        }
        // 默认解锁房间 0
        RoomInfo room0 = database.GetRoomInfo(0);
        if (room0 != null)
        {
            room0.unlocked = true;
            EnterRoom(0);
        }
        //UpdateAreaInstability();
        FindObjectOfType<MapUIController>()?.RefreshMap();
    }
    private void Update()
    {
        if (currentRoomID >= 0 && PlayerChaos.Instance != null)
        {
            chaosTickTimer += Time.deltaTime;
            if (chaosTickTimer >= 1f) // 每秒执行一次
            {
                chaosTickTimer = 0f;
                int growth = PlayerChaos.Instance.GetRoomInstabilityGrowth();
                if (growth > 0)
                {
                    RoomState state = GetRoomState(currentRoomID);
                    if (state != null)
                    {
                        state.instability += growth;
                        Debug.Log($"房间 {state.roomID} 因混乱度上升，+{growth} → {state.instability}");
                        UpdateAreaInstability(); // 区域平均逆恒也更新
                    }
                }
            }
        }
    }
    /// <summary>
    /// 进入指定房间
    /// </summary>
    public void EnterRoom(int roomID)
    {
        RoomInfo info = database.GetRoomInfo(roomID);
        RoomState state = GetRoomState(roomID);
        // 检查逆恒值是否达到临界
        if (state != null && state.instability >= state.instabilityThreshold)
        {
            Debug.Log($"房间 {roomID} 已达到临界值，无法进入！");
            if (GlobalDialogManager.Instance != null)
            {
                GlobalDialogManager.Instance.TriggerDialogue("1000");
            }
            return; // 阻止进入
        }

        if (info == null || info.prefab == null)
        {
            //Debug.LogError($"房间 {roomID} 在数据库中没有配置 prefab！");
            return;
        }

        if (!info.unlocked)
        {
            Debug.Log($"房间 {roomID} 未解锁，无法进入！");
            return;
        }

        // 卸载当前房间
        if (currentRoom != null)
        {
            // 如果当前房间是走廊 → 记录 UI 视角位置
            if (IsCorridor(currentRoomID))
            {
                MapRoomNavigator nav = currentRoom.GetComponentInChildren<MapRoomNavigator>();
                if (nav != null)
                {
                    lastCorridorViewPositions[currentRoomID] = nav.GetCurrentTargetPosition();
                    Debug.Log($"记录走廊 {currentRoomID} 的 targetPosition：{lastCorridorViewPositions[currentRoomID]}");
                }
            }
            Destroy(currentRoom);
        }

        // 实例化新房间
        currentRoom = Instantiate(info.prefab, transform);
        Room roomScript = currentRoom.GetComponent<Room>();
        currentRoom.transform.localPosition = roomScript.defaultPosition;
        currentRoomID = roomID;
        // 如果是走廊，并且之前记录过 → 恢复位置
        if (IsCorridor(roomID) && lastCorridorViewPositions.TryGetValue(roomID, out Vector2 savedPos))
        {
            MapRoomNavigator nav = currentRoom.GetComponentInChildren<MapRoomNavigator>();
            if (nav != null)
            {
                nav.hasRestoredPosition = true;   // 告诉它别再走默认 startPosition
                nav.MoveToPosition(savedPos, immediate: true);
                Debug.Log($"恢复走廊 {roomID} 的 targetPosition：{savedPos}");
            }
        }

        else
        {
            // 走默认初始化逻辑
            currentRoom.transform.localPosition = roomScript.defaultPosition;
        }
        ItemOnWorld[] items = currentRoom.GetComponentsInChildren<ItemOnWorld>(true);
        foreach (var worldItem in items)
        {
            if (worldItem.item is Level1Data level1Data)
            {
                // 根据数据对象的 isPicked 来决定是否显示
                worldItem.gameObject.SetActive(!level1Data.isPicked);
            }
        }

        //Debug.Log($"进入房间 {roomID}");
        // 更新混乱度逻辑
        if (state != null)
        {
            PlayerChaos.Instance.OnEnterRoom(state);
            // 检查是否首次进入
            if (!state.hasEntered)
            {
                state.hasEntered = true;
                TriggerRoomFirstEnterEvent(roomID);
            }
        }
        // 房间切换时更新 selectionUI
        if (GlobalDialogManager.Instance != null && GlobalDialogManager.Instance.selectionUI != null)
        {
            GlobalDialogManager.Instance.selectionUI.SetActive(roomID == 0);
        }
    }
    private bool IsCorridor(int roomID)
    {
        RoomInfo info = database.GetRoomInfo(roomID);
        return info != null && info.isCorridor; // 你需要在 RoomInfo 里加个 bool 标记走廊
    }
    private void TriggerRoomFirstEnterEvent(int roomID)
    {
        Debug.Log($"房间 {roomID} 首次进入，触发事件！");
        // 如果要在房间 prefab 里定义事件，可以这样：
        Room room = currentRoom.GetComponent<Room>();
        if (room != null)
        {
            room.onFirstEnter?.Invoke();
        }
    }

    /// <summary>
    /// 解锁房间
    /// </summary>
    public void UnlockRoom(int roomID)
    {
        RoomInfo info = database.GetRoomInfo(roomID);
        if (info != null && !info.unlocked)
        {
            info.unlocked = true;
            Debug.Log($"房间 {roomID} 已解锁！");
        }
    }

    /// <summary>
    /// 检查房间是否解锁
    /// </summary>
    public bool IsUnlocked(int roomID)
    {
        RoomInfo info = database.GetRoomInfo(roomID);
        return info != null && info.unlocked;
    }
    /// <summary>
    /// 提高某个房间的逆恒值
    /// </summary>
    public void IncreaseInstability(int roomID, int value)
    {
        if (roomStates.TryGetValue(roomID, out RoomState state))
        {
            state.instability += value;
            if (state.instability >= state.instabilityThreshold)
            {
                Debug.Log($"房间 {roomID} 已达到临界值，无法再次进入！");
            }
            UpdateAreaInstability();
            // 刷新地图 UI
            FindObjectOfType<MapUIController>()?.RefreshMap();
        }
    }
    /// <summary>
    /// 计算本区域的平均逆恒值
    /// </summary>
    public void UpdateAreaInstability()
    {
        float total = 0;
        int count = 0;

        foreach (int id in roomIDsInArea)
        {
            if (roomStates.TryGetValue(id, out RoomState state))
            {
                total += state.instability;
                count++;
            }
        }

        AreaInstability = (count > 0) ? total / count : 0f;
        Debug.Log($"区域平均逆恒值: {AreaInstability}");
    }
    public RoomState GetRoomState(int roomID)
    {
        return roomStates.TryGetValue(roomID, out RoomState state) ? state : null;
    }
    public Color GetInstabilityColor(int instability, int threshold)
    {
        //Debug.Log("GetInstabilityColor");
        int tmp = (instability - 9500) > 0 ? (instability - 9500) : 0;
        tmp = tmp > 500 ? 500 : tmp;
        // 归一化 0~1
        float t = Mathf.Clamp01((float)tmp / 500);

        // 蓝到红的渐变
        Color low = Color.white;
        Color high = Color.red;

        return Color.Lerp(low, high, t);
    }
    public string GetRoomName(int roomID)
    {
        // 优先从数据库里取
        RoomInfo info = database.GetRoomInfo(roomID);
        if (info != null && !string.IsNullOrEmpty(info.roomName))
            return info.roomName;

        // 如果数据库里没配，就退回到场景的 Room
        //if (roomDict.TryGetValue(roomID, out Room room))
        //    return room.roomName;

        return $"房间{roomID}"; // fallback
    }
}
