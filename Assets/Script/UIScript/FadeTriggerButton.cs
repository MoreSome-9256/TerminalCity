using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FadeTriggerButton : MonoBehaviour
{
    public enum FadeMode
    {
        FadeInAndOut, // 渐入后渐出（黑屏闪一下）
        FadeIn,       // 单纯淡入（渐变黑）
        FadeOut       // 单纯淡出（从黑恢复）
    }

    [Header("淡入淡出模式")]
    [SerializeField] private FadeMode mode = FadeMode.FadeInAndOut;

    [Tooltip("勾选后，该物品全局只触发一次黑屏")]
    [SerializeField] private bool triggerOnce = false;
    private bool hasTriggered = false;

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
        if (triggerOnce && hasTriggered) return;

        if (ObjectFadeController.Instance != null)
        {
            switch (mode)
            {
                case FadeMode.FadeInAndOut:
                    ObjectFadeController.Instance.FadeInAndOut();
                    break;
                case FadeMode.FadeIn:
                    ObjectFadeController.Instance.Fade(true);
                    break;
                case FadeMode.FadeOut:
                    ObjectFadeController.Instance.Fade(false);
                    break;
            }

            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 场景中未找到 ObjectFadeController 实例！");
        }
    }
}