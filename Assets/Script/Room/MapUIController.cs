using UnityEngine;
using UnityEngine.UI;

public class MapUIController : MonoBehaviour
{
    public RoomButton[] roomButtons;

    public void RefreshMap()
    {
        foreach (var rb in roomButtons)
        {
            RoomState state = RoomManager.Instance.GetRoomState(rb.roomID);
            if (state != null)
            {
                // 更新颜色
                Color c = RoomManager.Instance.GetInstabilityColor(state.instability, state.instabilityThreshold);
                rb.UpdateColor(c);

                // 超过阈值则禁用按钮，否则启用
                Button btn = rb.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = state.instability < state.instabilityThreshold;
                }
            }
        }
    }
}
