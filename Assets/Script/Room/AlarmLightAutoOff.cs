using UnityEngine;

public class AlarmLightAutoOff : MonoBehaviour
{
    [Header("绑定的触发房间ID（离开该房间后自动关闭）")]
    [Tooltip("例如第一个房间的 roomID 为 1")]
    [SerializeField] private int targetRoomID = 1;

    private int boundRoomID = -1;

    private void OnEnable()
    {
        // 每次灯光被打开时，记录下当前是在哪个房间触发的
        if (RoomManager.Instance != null)
        {
            boundRoomID = RoomManager.Instance.currentRoomID;
        }
    }

    private void Update()
    {
        // 如果 RoomManager 已经切换到了其他房间（或者当前房间不再是绑定的房间）
        if (RoomManager.Instance != null && boundRoomID != -1)
        {
            if (RoomManager.Instance.currentRoomID != boundRoomID)
            {
                // 离开房间，自动关闭自身
                gameObject.SetActive(false);
                boundRoomID = -1;
            }
        }
    }
}