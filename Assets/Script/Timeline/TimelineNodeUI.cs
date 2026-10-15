using UnityEngine;
using TMPro;

public class TimelineNodeUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TextMeshProUGUI dateText;      // 对应 DateText[cite: 19]
    [SerializeField] private RectTransform tickLine;        // 对应 TickLine[cite: 19]
    [SerializeField] private TextMeshProUGUI eventDescText; // 对应 EventDescText[cite: 19]
    [SerializeField] private GameObject newPrompt;          // 对应 NewPrompt[cite: 19]

    /// <summary>
    /// 初始化填充节点数据与提示状态
    /// </summary>
    public void Setup(TimelineEventData item, bool isNew)
    {
        if (dateText != null)
        {
            dateText.text = item.dateDisplay;
        }

        if (eventDescText != null)
        {
            eventDescText.text = item.description;
        }

        if (newPrompt != null)
        {
            newPrompt.SetActive(isNew);
        }
    }
}