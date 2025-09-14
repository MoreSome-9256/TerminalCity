using UnityEngine;

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
                Color c = RoomManager.Instance.GetInstabilityColor(state.instability, state.instabilityThreshold);
                rb.UpdateColor(c);
            }
        }
    }
}
