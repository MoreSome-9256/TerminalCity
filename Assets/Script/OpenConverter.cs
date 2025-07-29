using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class OpenConverter : MonoBehaviour, IPointerClickHandler
{
    public GameObject converter;
    public void OnPointerClick(PointerEventData eventData)
    {
        converter.gameObject.SetActive(true);
    }
}
