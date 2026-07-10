using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreView : MonoBehaviour
{
    static PreView instance;

    public Slot slotPrefab;
    public Image image;
    public TMP_Text name;
    public TMP_Text trait;
    public TMP_Text info;

    public GameObject fastBag;
    public GameObject message;
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        instance = this;

    }

    public static void ShowInformation(Item item)
    {
        if (instance.fastBag != null)
        {
            instance.fastBag.SetActive(false);
        }
        if (instance.message != null)
        {
            instance.message.SetActive(false);
        }
        instance.image.sprite = item.itemImage;
        instance.name.text = item.LocalizedItemName;
        instance.info.text = item.LocalizedItemInfo;
        if (item is Level1Data level1Data)
        {
            string traits = "";
            if (!string.IsNullOrEmpty(level1Data.Name))
            {
                traits += "人物 ";
            }
            if (!string.IsNullOrEmpty(level1Data.Time))
            {
                traits += "时间 ";
            }
            if (!string.IsNullOrEmpty(level1Data.Event))
            {
                traits += "事件 ";
            }
            if (string.IsNullOrEmpty(level1Data.Name) && string.IsNullOrEmpty(level1Data.Time) && string.IsNullOrEmpty(level1Data.Event))
            {
                traits = "无";
            }
            instance.trait.text = traits.Trim();
        }
    }
}
