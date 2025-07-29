using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewData : MonoBehaviour
{
    static NewData instance;
    public Slot slotPrefab;
    public Image image;
    public TMP_Text name;

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
        instance.image.sprite = item.itemImage;
        instance.name.text = item.itemName;
    }
}
