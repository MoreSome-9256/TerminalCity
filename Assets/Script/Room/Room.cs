using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Room : MonoBehaviour
{
    [Header("房间ID（全局唯一）")]
    public int roomID;

    //public string roomName;
    // ✨ 新增：允许在 Prefab 上直接配置该房间的初始逆恒值和阈值
    [Header("逆恒值配置")]
    public int defaultInstability = 9500;
    public int instabilityThreshold = 10000;

    public Vector3 defaultPosition = Vector3.zero;

    [Header("可通往的其他房间ID")]
    public List<int> connectedRoomIDs = new List<int>();
    [Header("首次进入触发的事件列表")]
    public UnityEvent onFirstEnter;
    [Header("离开房间时触发的事件列表")]
    public UnityEvent onLeave; // ? 新增离开事件

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
    // 代理方法，可以在 Inspector 里直接绑定
    public void TriggerGlobalDialog(string dialogID)
    {
        GlobalDialogManager.Instance?.TriggerDialogue(dialogID);
    }
    /// <summary>
    /// 离开房间时调用
    /// </summary>
    public void LeaveRoom()
    {
        onLeave?.Invoke();
    }
}
