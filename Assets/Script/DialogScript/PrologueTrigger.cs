using UnityEngine;

public class PrologueTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetDialogue; // 直接把场景里的 Dialogue1 拖进来

    private void OnEnable()
    {
        PiGlitchAndCollapsePlotPoint.OnOpeningFinished += EnableDialogue;
    }

    private void OnDisable()
    {
        PiGlitchAndCollapsePlotPoint.OnOpeningFinished -= EnableDialogue;
    }

    private void EnableDialogue()
    {
        if (targetDialogue != null)
        {
            targetDialogue.SetActive(true);
        }
    }
}