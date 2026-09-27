using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewMessage : MonoBehaviour
{
    public GameObject messageArea;
    public TMP_Text newText;
    public string message;
    public GameObject LoadingBar;
    public bool isBarShow = false;

    public bool triggerOnce = true;
    private bool hasTriggered = false;

    public void ShowMessage()
    {
        Debug.Log("show message,hasTriggered=" + hasTriggered);
        if (triggerOnce && hasTriggered)
        {
            Debug.Log("has triggered");
            return; // 已触发过且只允许一次，直接跳出
        }

        hasTriggered = true;
        Debug.Log("hasTriggered=" + hasTriggered);
        messageArea.SetActive(true);
        newText.gameObject.SetActive(true);
        newText.text = message;

        if (isBarShow)
        {
            LoadingBar.SetActive(true);
        }
        else
        {
            LoadingBar.SetActive(false);
        }
    }
}
