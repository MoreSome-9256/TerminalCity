using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class StateGateInteractable : MonoBehaviour
{
    [Header("前置条件（纯字符串，对应 GlobalDialogManager.triggeredFlags）")]
    [Tooltip("必须全部包含在 triggeredFlags 中才算满足条件")]
    [SerializeField] private List<string> requiredFlags = new List<string>();

    [Header("点击成功后写入的新标记（可选）")]
    [Tooltip("例如开门成功后写入 DOOR_OPENED")]
    [SerializeField] private string flagToSetOnSuccess;

    [Header("互斥与限制")]
    [SerializeField] private bool interactOnce = false;
    private bool hasInteracted = false;

    [Header("事件响应（仅在当前 Prefab 内部连线，或调本物体的动画/对话）")]
    [Tooltip("条件满足时触发（如走廊移动、开门动画）")]
    public UnityEvent onSuccessInteract;

    [Tooltip("条件不足时触发（如提示'先探索xxx房间吧'的对话）")]
    public UnityEvent onFailInteract;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (interactOnce && hasInteracted) return;

        // 直接用项目现有的 GlobalDialogManager.triggeredFlags 判断
        bool conditionsMet = CheckConditionsMet();

        if (conditionsMet)
        {
            hasInteracted = true;

            // 写入成功标记
            if (!string.IsNullOrEmpty(flagToSetOnSuccess))
            {
                if (!GlobalDialogManager.triggeredFlags.Contains(flagToSetOnSuccess))
                {
                    GlobalDialogManager.triggeredFlags.Add(flagToSetOnSuccess);
                }
            }

            onSuccessInteract?.Invoke();
        }
        else
        {
            onFailInteract?.Invoke();
        }
    }

    private bool CheckConditionsMet()
    {
        if (requiredFlags == null || requiredFlags.Count == 0) return true;

        foreach (string flag in requiredFlags)
        {
            if (string.IsNullOrEmpty(flag)) continue;

            // 如果静态列表里没有这个 flag，判定未满足
            if (!GlobalDialogManager.triggeredFlags.Contains(flag))
            {
                return false;
            }
        }
        return true;
    }
}