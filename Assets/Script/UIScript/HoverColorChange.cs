using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image changeObject;
    [Range(0.1f, 2f)] public float transitionDuration = 0.1f; // 可调节过渡时间

    private Color initialColor;
    private Color unhover;
    private Coroutine colorTransition;

    public float alpahThreshold = 0.5f;

    void Start()
    {
        initialColor = changeObject.color;
        unhover = new Color32(175, 175, 175, 255);
        changeObject.color = unhover;
        changeObject.GetComponent<Image>().alphaHitTestMinimumThreshold = alpahThreshold;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartColorTransition(initialColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartColorTransition(unhover);
    }

    private void StartColorTransition(Color targetColor)
    {
        if (colorTransition != null)
            StopCoroutine(colorTransition);

        colorTransition = StartCoroutine(ColorTransition(targetColor));
    }

    private IEnumerator ColorTransition(Color target)
    {
        float elapsedTime = 0f;
        Color startColor = changeObject.color;

        while (elapsedTime < transitionDuration)
        {
            changeObject.color = Color.Lerp(
                startColor,
                target,
                elapsedTime / transitionDuration
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        changeObject.color = target;
    }
}