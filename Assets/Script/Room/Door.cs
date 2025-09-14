using UnityEngine;

public class Door : MonoBehaviour
{
    public int targetRoomID;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 右键
        {
            ShowRoomInfo();
        }
    }

    private void ShowRoomInfo()
    {
        RoomState state = RoomManager.Instance.GetRoomState(targetRoomID);
        if (state == null) return;

        UIManager ui = UIManager.Instance;
        if (ui == null) return;

        SidePanelManager.Instance.ShowPanel(ui.roomInfoPanel);
        // 一次性拿到描述 + Sprite
        InstabilityInfo info = InstabilityTextProvider.GetInfo(state.instability, state.instabilityThreshold);

        ui.instabilityText.text = $"{state.instability}";
        ui.descriptionText.text = info.description;
        if (info.profileSprite != null)
            ui.npcImage.sprite = info.profileSprite;
    }
}