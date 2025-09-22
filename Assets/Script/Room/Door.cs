using UnityEngine;
using UnityEngine.EventSystems;

public class Door : MonoBehaviour, IPointerClickHandler
{
    public int targetRoomID;

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log($"[{name}] ±»µã»÷£¬°´¼ü={eventData.button}");
        if (eventData.button == PointerEventData.InputButton.Right) // ÓÒ¼ü
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
        InstabilityInfo info = InstabilityTextProvider.GetInfo(state.instability, state.instabilityThreshold);

        ui.instabilityText.text = $"{state.instability}";
        ui.descriptionText.text = info.description;
        if (info.profileSprite != null)
            ui.npcImage.sprite = info.profileSprite;
    }
}
