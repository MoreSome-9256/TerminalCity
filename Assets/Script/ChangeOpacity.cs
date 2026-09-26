using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeOpacity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("目标图片列表")]
    [SerializeField] private List<Image> targetImages = new List<Image>();

    [Header("透明度设置")]
    [Range(0, 1)]
    [SerializeField] private float normalAlpha = 0.5f;   // 默认透明度
    [Range(0, 1)]
    [SerializeField] private float hoverAlpha = 1f;       // 悬停透明度

    void Awake()
    {
        // 确保当前挂载脚本的物体自身如果有 Image，可以作为触发检测区域
        Image selfImage = GetComponent<Image>();
        if (selfImage != null)
        {
            selfImage.raycastTarget = true;
            // 如果希望自身也受透明度控制且未加入列表，可自动添加
            if (!targetImages.Contains(selfImage))
            {
                targetImages.Add(selfImage);
            }
        }

        // 初始化所有目标的透明度
        SetAlpha(normalAlpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(hoverAlpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(normalAlpha);
    }

    private void SetAlpha(float alpha)
    {
        foreach (var img in targetImages)
        {
            if (img != null)
            {
                Color newColor = img.color;
                newColor.a = alpha;
                img.color = newColor;
            }
        }
    }
}