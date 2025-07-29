using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItemDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject infoWindow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        infoWindow.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        infoWindow.SetActive(false);
    }
}