using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("房间ID（全局唯一）")]
    public int roomID;

    public Vector3 defaultPosition = Vector3.zero;

    [Header("可通往的其他房间ID")]
    public List<int> connectedRoomIDs = new List<int>();

    /// <summary>
    /// 点击房间内的门时调用
    /// </summary>
    public void OnDoorClicked(int targetRoomID)
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.EnterRoom(targetRoomID);
        }
        else
        {
            Debug.LogError("RoomManager.Instance 为空，无法切换房间！");
        }
    }
}
