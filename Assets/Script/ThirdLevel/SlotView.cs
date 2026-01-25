using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    public Image icon;
    public TMP_Text nameText;

    [Header("Locked State")]
    public Sprite lockedSprite;   // ºÚÎíÌùÍ¼
    public string lockedName = "???";

    // Runtime
    private Level3Data data;
    private bool unlocked;

    /// <summary>
    /// Ë¢ÐÂ²ÛÎ»ÏÔÊ¾
    /// </summary>
    public void Refresh(Level3Data level3Data, bool isUnlocked)
    {
        data = level3Data;
        unlocked = isUnlocked;

        if (unlocked)
        {
            icon.sprite = data.itemImage;
            icon.enabled = true;
            nameText.text = data.itemName;
        }
        else
        {
            if (lockedSprite != null)
            {
                icon.sprite = lockedSprite;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }

            nameText.text = lockedName;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!unlocked) return;

        CharacterDetailController controller =
            FindObjectOfType<CharacterDetailController>();

        if (controller != null)
        {
            controller.OnSlotClicked(data);
        }
    }
}
