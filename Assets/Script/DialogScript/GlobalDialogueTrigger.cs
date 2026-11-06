using UnityEngine;

/// <summary>
/// 通用对话触发器，专用于在 Prefab 内部或场景中调用 GlobalDialogManager
/// </summary>
public class GlobalDialogueTrigger : MonoBehaviour
{
    [Header("对话配置")]
    [Tooltip("填入数据库中的起始对话 ID")]
    public string dialogueID;

    /// <summary>
    /// 触发对话，可绑定到 Button 的 OnClick 事件，或由其他交互脚本直接调用
    /// </summary>
    public void Trigger()
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning($"<color=yellow>[DialogueTrigger]</color> {gameObject.name} 的 dialogueID 为空，无法触发对话！");
            return;
        }

        // 调用 GlobalDialogManager 的 TriggerDialogue 方法
        if (GlobalDialogManager.Instance != null)
        {
            GlobalDialogManager.Instance.TriggerDialogue(dialogueID); //[cite: 18]
        }
        else
        {
            Debug.LogError("<color=red>[DialogueTrigger]</color> 场景中未找到 GlobalDialogManager 的实例！请确保它已放置在场景中。");
        }
    }
}