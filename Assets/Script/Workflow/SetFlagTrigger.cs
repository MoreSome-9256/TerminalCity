using UnityEngine;
using UnityEngine.UI;

public class SetFlagTrigger : MonoBehaviour
{
    [Header("要点亮的标记名称（直接填字符串）")]
    [SerializeField] private string flagToSet;

    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(TriggerSetFlag);
        }
    }

    /// <summary>
    /// 公开方法：既可以挂在 Button 上点击触发，也可以挂在 Dialogue 的 onDialogueEnd 里
    /// </summary>
    public void TriggerSetFlag()
    {
        if (triggerOnce && hasTriggered) return;
        if (string.IsNullOrEmpty(flagToSet)) return;

        if (!GlobalDialogManager.triggeredFlags.Contains(flagToSet))
        {
            GlobalDialogManager.triggeredFlags.Add(flagToSet);
            Debug.Log($"<color=#00FF00>[GlobalFlag] 写入标记: {flagToSet}</color>");
        }

        hasTriggered = true;
    }
}