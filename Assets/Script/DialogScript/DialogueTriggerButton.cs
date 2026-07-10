using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DialogueTriggerButton : MonoBehaviour
{
    [Header("对话配置")]
    [Tooltip("要触发的首句对话ID")]
    [SerializeField] private string dialogueID;

    [Tooltip("勾选后，该ID的对话无论在哪个物体上，全局都只能被触发一次")]
    [SerializeField] private bool triggerOnce = false;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        // 绑定点击事件
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning($"[{gameObject.name}] 对话ID为空，无法触发对话！");
            return;
        }

        // 如果开启了“仅触发一次”的限制
        if (triggerOnce)
        {
            // 利用 GlobalDialogManager 现有的静态列表 triggeredFlags 来记录全局触发过的 Flag
            // 这里给 ID 加上前缀以防与数据库的普通条件 Flag 混淆
            string globalFlagKey = "ONCE_TRIGGER_" + dialogueID;

            if (GlobalDialogManager.triggeredFlags.Contains(globalFlagKey))
            {
                Debug.Log($"[{gameObject.name}] 该类型的对话 '{dialogueID}' 全局已经触发过一次，不再触发。");
                return;
            }

            // 第一次触发，记录到全局去重列表中
            GlobalDialogManager.triggeredFlags.Add(globalFlagKey);
        }

        // 调用全局对话管理器的实例来启动对话
        if (GlobalDialogManager.Instance != null)
        {
            GlobalDialogManager.Instance.TriggerDialogue(dialogueID);
        }
        else
        {
            Debug.LogError("[DialogueTriggerButton] 未能在场景中找到 GlobalDialogManager 实例！");
        }
    }
}