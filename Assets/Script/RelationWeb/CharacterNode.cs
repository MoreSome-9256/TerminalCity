using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterNode : MonoBehaviour, IPointerClickHandler
{
    public Image PortraitImage;
    public TMP_Text NameText;

    public string CharacterName;

    private RelationshipGraph graph;
    private bool isSelected = false;
    public Image SelectedImage;
    public Image BorderImage;
    public Sprite normalBorderSprite;
    public Sprite selectedBorderSprite;
    private void Awake()
    {
        graph = FindObjectOfType<RelationshipGraph>();
        if (SelectedImage != null)
            SelectedImage.gameObject.SetActive(false);

        if (BorderImage != null && normalBorderSprite != null)
            BorderImage.sprite = normalBorderSprite;
    }
    public void Init(CharacterInfo info)
    {
        CharacterName = info.Name;
        PortraitImage.sprite = info.Portrait;
        NameText.text = info.Name;
        GetComponent<RectTransform>().anchoredPosition = info.DefaultPosition;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[Click] {CharacterName}");
        isSelected = !isSelected;
        graph.OnNodeClicked(this, isSelected);
        SetSelectedVisual(isSelected);
    }
    public void SetSelectedVisual(bool selected)
    {
        if (SelectedImage != null)
            SelectedImage.gameObject.SetActive(selected);

        if (BorderImage != null)
            BorderImage.sprite = selected ? selectedBorderSprite : normalBorderSprite;
    }
    public void SetIsSelected(bool isSelected)
    {
        this.isSelected = isSelected;
    }
}
