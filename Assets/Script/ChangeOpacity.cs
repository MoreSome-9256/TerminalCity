using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChangeOpacity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage;
    [Header("透明度设置")]
    [Range(0, 1)]
    [SerializeField] private float normalAlpha = 0.5f;   // 默认透明度
    [Range(0, 1)]
    [SerializeField] private float hoverAlpha = 1f;  // 悬停透明度

    void Awake()
    {
        targetImage = GetComponent<Image>();
        targetImage.raycastTarget = true;
        SetAlpha(normalAlpha); // 初始化透明度
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
        Color newColor = targetImage.color;
        newColor.a = alpha;
        targetImage.color = newColor;
    }
    
}
