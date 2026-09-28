using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class BagCloseDialogueManagerTrigger : MonoBehaviour
{
    [Header("绑定背包")]
    [Tooltip("拖入移动的背包面板（带有 RectTransform）")]
    [SerializeField] private RectTransform bagRect;

    [Header("目标对话组件")]
    [Tooltip("拖入挂载了台词的 DialogueManager")]
    [SerializeField] private DialogueManager targetDialogueManager;

    [Header("触发限制")]
    [Tooltip("是否只触发一次")]
    [SerializeField] private bool triggerOnce = true;

    [Header("判定容差 (像素)")]
    [Tooltip("当距离初始隐藏位置小于该距离时，判定为已完全关闭")]
    [SerializeField] private float closeThreshold = 5f;

    private Button button;
    private Vector2 closedPosition; // 记录背包完全收回时的坐标
    private bool hasTriggered = false;
    private Coroutine waitCoroutine;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (bagRect != null)
        {
            // 记录游戏刚开始时，背包在屏幕外的初始收回坐标
            closedPosition = bagRect.anchoredPosition;
        }

        button.onClick.AddListener(OnBagButtonClicked);
    }

    private void OnBagButtonClicked()
    {
        if (triggerOnce && hasTriggered) return;

        // 如果已经在监听中，不重复开启协程
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
        }

        waitCoroutine = StartCoroutine(WaitBagCycleRoutine());
    }

    private IEnumerator WaitBagCycleRoutine()
    {
        // 1. 等待背包离开初始位置（确认开始向屏幕内滑出了）
        while (Vector2.Distance(bagRect.anchoredPosition, closedPosition) <= closeThreshold)
        {
            yield return null;
        }

        // 2. 轮询等待背包重新滑回到初始位置（确认完全收回屏幕外了）
        while (Vector2.Distance(bagRect.anchoredPosition, closedPosition) > closeThreshold)
        {
            yield return null;
        }

        // 3. 标记已触发并启动对话
        hasTriggered = true;
        waitCoroutine = null;

        if (targetDialogueManager != null)
        {
            if (!targetDialogueManager.gameObject.activeInHierarchy)
            {
                targetDialogueManager.gameObject.SetActive(true);
            }
            targetDialogueManager.StartDialogue();
        }
        else
        {
            Debug.LogError("[BagTrigger] 未绑定 targetDialogueManager！");
        }
    }
}