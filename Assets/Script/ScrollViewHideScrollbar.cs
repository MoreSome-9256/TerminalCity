using UnityEngine;
using UnityEngine.UI;

public class ScrollViewHideScrollbar : MonoBehaviour
{
    [Header("滚动视图配置")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar verticalScrollbar;
    [SerializeField] private Scrollbar horizontalScrollbar;

    void Start()
    {
        // 隐藏滚动条但保持功能
        ConfigureScrollbar(verticalScrollbar);
        ConfigureScrollbar(horizontalScrollbar);

        // 可选：强制设置滚动策略
        scrollRect.vertical = verticalScrollbar != null;
        scrollRect.horizontal = horizontalScrollbar != null;
    }

    private void ConfigureScrollbar(Scrollbar scrollbar)
    {
        if (scrollbar == null) return;

        var scrollbarImages = scrollbar.GetComponentsInChildren<Image>();
        foreach (var image in scrollbarImages)
        {
            image.enabled = false; // 禁用所有子图像
        }
    }

    // 动态控制方法
    public void ToggleVerticalScroll(bool enable)
    {
        if (verticalScrollbar)
        {
            verticalScrollbar.gameObject.SetActive(enable);
            scrollRect.vertical = enable;
        }
    }
}