using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour
{
    public int roomID;
    private Button button;
    [SerializeField] private Image targetImage;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (targetImage == null)
            targetImage = GetComponent<Image>(); // ¶µµ×

        button.onClick.AddListener(() =>
        {
            RoomManager.Instance.EnterRoom(roomID);
        });
    }

    public void UpdateColor(Color c)
    {
        if (targetImage != null)
            targetImage.color = c;
    }

}
