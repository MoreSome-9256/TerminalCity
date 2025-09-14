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

    public float AreaInstability { get; private set; }

    private GameObject currentRoom;
    public int currentRoomID = -1;

    private float chaosTickTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(this);

        // 默认解锁房间 0
        RoomInfo room0 = database.GetRoomInfo(0);
        if (room0 != null)
        {
            room0.unlocked = true;
            EnterRoom(0);
        }
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
                if (id == 0) defaultInstability = 9700;
                if (id == 4) defaultInstability = 9900;

                roomStates[id] = new RoomState(id, defaultInstability, threshold);
            }
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
            Destroy(currentRoom);
        }

        // 实例化新房间
        currentRoom = Instantiate(info.prefab, transform);
        Room roomScript = currentRoom.GetComponent<Room>();
        currentRoom.transform.localPosition = roomScript.defaultPosition;
        currentRoomID = roomID;
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
        RoomState state = GetRoomState(roomID);
        if (state != null)
        {
            PlayerChaos.Instance.OnEnterRoom(state);
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

}
