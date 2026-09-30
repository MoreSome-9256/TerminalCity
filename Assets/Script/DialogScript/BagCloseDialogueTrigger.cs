using UnityEngine;
using UnityEngine.UI;

public class BagCloseDialogueManagerTrigger : MonoBehaviour
{
    [Header("绑定背包滑动/开关控制器")]
    [Tooltip("拖入带有 SideScreenMove 脚本的背包对象")]
    [SerializeField] private SideScreenMove bagController;

    [Header("背包开关键（确保恢复显示）")]
    [Tooltip("拖入开关背包的 Button，解决对话结束后按钮不显示的 Bug")]
    [SerializeField] private GameObject bagOpenButton;

    [Header("目标对话组件")]
    [Tooltip("拖入挂载了台词的 DialogueManager")]
    [SerializeField] private DialogueManager targetDialogueManager;

    [Header("触发限制")]
    [Tooltip("是否只触发一次")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        if (bagController != null)
        {
            // 直接监听背包完全关闭的官方事件！既准又稳，保证动画和Mask全部关完
            bagController.OnBagClosed.AddListener(OnBagFullyClosed);
        }
        else
        {
            Debug.LogError("[BagTrigger] 请指定 Bag Controller (SideScreenMove)！");
        }

        // 监听对话结束事件，强制把开关键重新点亮显示出来[cite: 13]
        if (targetDialogueManager != null)
        {
            targetDialogueManager.onDialogueEnd.AddListener(OnDialogueFinished);
        }
    }

    private void OnDestroy()
    {
        if (bagController != null)
        {
            bagController.OnBagClosed.RemoveListener(OnBagFullyClosed);
        }

        if (targetDialogueManager != null)
        {
            targetDialogueManager.onDialogueEnd.RemoveListener(OnDialogueFinished);
        }
    }

    /// <summary>
    /// 背包完全收回到屏幕外并处理完 Mask 之后触发[cite: 19]
    /// </summary>
    private void OnBagFullyClosed()
    {
        if (triggerOnce && hasTriggered) return;

        // 补刀保险：确保 Mask 被彻底关闭[cite: 19]
        if (bagController.mask != null && bagController.mask.activeSelf)
        {
            bagController.mask.SetActive(false);
        }

        hasTriggered = true;

        // 启动对话[cite: 13]
        if (targetDialogueManager != null)
        {
            if (!targetDialogueManager.gameObject.activeInHierarchy)
            {
                targetDialogueManager.gameObject.SetActive(true);
            }
            targetDialogueManager.StartDialogue();
        }
    }

    /// <summary>
    /// 对话完全结束时触发（解决问题一）
    /// </summary>
    private void OnDialogueFinished()
    {
        // 强制重新激活背包按钮，防止被 DialogueManager 藏起来后没恢复
        if (bagOpenButton != null)
        {
            bagOpenButton.SetActive(true);
            Button btn = bagOpenButton.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }
    }
}