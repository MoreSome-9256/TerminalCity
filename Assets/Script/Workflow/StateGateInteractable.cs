using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class StateGateInteractable : MonoBehaviour
{
    [Header("前置条件（必须全部满足才能互动）")]
    [Tooltip("如果为空，则随时可点击。如果填了 KEY_BASEMENT，就必须在黑板有该标记时才能点")]
    [SerializeField] private List<string> requiredFlags = new List<string>();

    [Header("点击成功后点亮的新标记（可选）")]
    [Tooltip("例如点完这个日记本，点亮 FLAG_READ_DIARY，供后续剧情使用")]
    [SerializeField] private string flagToSetOnClick;

    [Header("互斥/只触发一次")]
    [SerializeField] private bool interactOnce = false;
    private bool hasInteracted = false;

    [Header("交互行为")]
    [Tooltip("条件满足时点击触发（比如播对话、打开门等）")]
    public UnityEvent onSuccessInteract;

    [Tooltip("条件不足时点击触发（比如弹出一句台词：'门锁着，需要钥匙'）")]
    public UnityEvent onFailInteract;

    [Header("表现层控制（可选）")]
    [Tooltip("如果条件不满足，是否直接把该物体的 Button 禁用（无法点击）或者直接隐藏 GameObject")]
    [SerializeField] private bool hideIfConditionsNotMet = false;
    [SerializeField] private bool disableButtonIfNotMet = false;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += RefreshVisualState;
        RefreshVisualState();
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= RefreshVisualState;
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
        RefreshVisualState();
    }

    /// <summary>
    /// 当全场任何状态变化时，自己核对一下当前自己能不能显现/交互
    /// </summary>
    public void RefreshVisualState()
    {
        if (GameStateManager.Instance == null) return;

        bool conditionsMet = GameStateManager.Instance.CheckConditions(requiredFlags);

        if (hideIfConditionsNotMet)
        {
            gameObject.SetActive(conditionsMet);
        }
        else if (disableButtonIfNotMet)
        {
            button.interactable = conditionsMet && (!interactOnce || !hasInteracted);
        }
    }

    private void OnClick()
    {
        if (interactOnce && hasInteracted) return;

        bool conditionsMet = GameStateManager.Instance != null &&
                             GameStateManager.Instance.CheckConditions(requiredFlags);

        if (conditionsMet)
        {
            hasInteracted = true;

            // 1. 点亮新标记
            if (!string.IsNullOrEmpty(flagToSetOnClick) && GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetFlag(flagToSetOnClick);
            }

            // 2. 触发对应事件（对话、开门动画等）
            onSuccessInteract?.Invoke();

            if (disableButtonIfNotMet && interactOnce)
            {
                button.interactable = false;
            }
        }
        else
        {
            // 条件不符时的反馈
            onFailInteract?.Invoke();
        }
    }
}