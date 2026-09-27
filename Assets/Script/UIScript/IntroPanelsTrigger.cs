using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class IntroPanelsTrigger : MonoBehaviour
{
    [Tooltip("勾选后，全局只会触发一次")]
    [SerializeField] private bool triggerOnce = true;

    // 静态标记：保证无论该 Prefab 怎么重复实例化/切换，全局只触发一次
    private static bool hasTriggeredGlobal = false;

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
        // 如果开启了一次性触发且已经触发过，直接拦截
        if (triggerOnce && hasTriggeredGlobal)
        {
            return;
        }

        if (IntroPanelsController.Instance != null)
        {
            IntroPanelsController.Instance.ActivateIntroPanels();
            hasTriggeredGlobal = true;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 当前场景未找到 IntroPanelsController 实例！");
        }
    }

    /// <summary>
    /// 可选：如果重新开始游戏/重置状态时需要复位，可以调用此方法
    /// </summary>
    public static void ResetTriggerState()
    {
        hasTriggeredGlobal = false;
    }
}