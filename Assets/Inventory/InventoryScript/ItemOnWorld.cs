using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ItemOnWorld : MonoBehaviour, IPointerClickHandler
{
    private RelationshipGraph relationshipGraph;

    public Level1Data item;
    public Inventory playerInventory;
    public GameObject previewArea;
    public GameObject firstLevel;
    public UnityEvent onClick;

    public AudioClip clickSound;
    private AudioSource audioSource;

    private void Awake()
    {
        relationshipGraph = FindObjectOfType<RelationshipGraph>();

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        ShowPreView();
        AddNewItem();
        AddGraph();
        Destroy(gameObject);
        onClick?.Invoke();
    }
    public void AddNewItem()
    {
        firstLevel.SetActive(true);
        playerInventory.level1List.Add(item);
        InventoryManager.CreateNewItem(item);
        InventoryManager2.CreateNewItem(item);
        firstLevel.SetActive(false);
    }
    public void ShowPreView()
    {
        previewArea.SetActive(true);
        PreView.ShowInformation(item);
    }
    public void AddGraph()
    {
        if (item is Level1Data level1Data)
        {
            relationshipGraph.UnlockCharacterByLevel1Data(level1Data);
        }
    }
}
