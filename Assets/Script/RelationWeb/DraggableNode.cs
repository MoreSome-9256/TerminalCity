using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableNode : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition += eventData.delta;
    }
}
