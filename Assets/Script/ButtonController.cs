using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private Button targetButton;
    public bool isActive = true;

    void Awake()
    {
        if (!isActive)
        {
            targetButton.interactable = false;
        }
        else
        {
            targetButton.interactable = true;
        }
    }
    public void DisableButton()
    {
        targetButton.interactable = false;
    }

    public void EnableButton()
    {
        targetButton.interactable = true;
    }
}