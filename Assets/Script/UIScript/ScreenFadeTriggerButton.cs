using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ScreenFadeTriggerButton : MonoBehaviour
{
    public enum ScreenFadeType
    {
        FadeInOut,
        FadeToBlack
    }

    [Header("转场模式")]
    [SerializeField] private ScreenFadeType fadeType = ScreenFadeType.FadeInOut;

    [Header("时长配置（若为 -1 则使用默认设置）")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float stayDuration = 0.2f;

    [Header("触发限制")]
    [Tooltip("勾选后，该物品全局只触发一次")]
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

        if (ScreenFader.Instance != null)
        {
            if (fadeType == ScreenFadeType.FadeInOut)
            {
                ScreenFader.Instance.FadeInOut(fadeDuration, stayDuration);
            }
            else
            {
                ScreenFader.Instance.FadeToBlack(fadeDuration);
            }

            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 场景中未找到 ScreenFader 实例！");
        }
    }
}