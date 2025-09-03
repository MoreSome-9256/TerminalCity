using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("房间数据库（ScriptableObject）")]
    public RoomDatabase database;

    private GameObject currentRoom;
    public int currentRoomID = -1;

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

        //Debug.Log($"进入房间 {roomID}");
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
}
