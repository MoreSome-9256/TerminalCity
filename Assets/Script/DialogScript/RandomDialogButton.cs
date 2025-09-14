using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RandomDialogButton : MonoBehaviour, IPointerClickHandler
{
    public List<int> candidateIDs;
    public void OnPointerClick(PointerEventData eventData)
    {
        GlobalDialogManager.Instance.TriggerRandomOpening(candidateIDs);
    }
}
