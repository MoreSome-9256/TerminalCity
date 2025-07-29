using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SidebarMenuController : MonoBehaviour
{
    public Button[] primaryButtons;
    public GameObject[] secondaryMenus;
    public RectTransform fourthButtonRectTransform;
    public GameObject[] openButton;

    private RectTransform rectTransform;
    private Vector2 originalFourthButtonPosition;
    private Quaternion originalRotation1;
    private Quaternion originalRotation2;
    private bool isMenuOpen = false;
    private bool isRotated1 = false;
    private bool isRotated2 = false;

    private bool[] lastMenuStates;
    private Coroutine[] rotateCoroutines;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalFourthButtonPosition = fourthButtonRectTransform.anchoredPosition;
        originalRotation1 = openButton[0].transform.localRotation;
        originalRotation2 = openButton[0].transform.localRotation;

        primaryButtons[2].onClick.AddListener(() => ToggleSecondaryMenu(0, primaryButtons[3].gameObject));
        primaryButtons[3].onClick.AddListener(() => ToggleSecondaryMenu(1));

        lastMenuStates = new bool[secondaryMenus.Length];
        rotateCoroutines = new Coroutine[secondaryMenus.Length];

        for (int i = 0; i < secondaryMenus.Length; i++)
        {
            lastMenuStates[i] = secondaryMenus[i].activeInHierarchy;

            // 初始化角度一致（避免刚进来角度错乱）
            openButton[i].transform.localRotation = Quaternion.Euler(0, 0, lastMenuStates[i] ? -90f : 0f);
        }
    }

    void Update()
    {
        for (int i = 0; i < secondaryMenus.Length; i++)
        {
            bool currentState = secondaryMenus[i].activeInHierarchy;

            if (currentState != lastMenuStates[i])
            {
                // 仅停止该按钮的旋转协程
                if (rotateCoroutines[i] != null)
                {
                    StopCoroutine(rotateCoroutines[i]);
                }

                rotateCoroutines[i] = StartCoroutine(SyncRotateButton(openButton[i], currentState ? -90f : 0f, 0.15f));
                lastMenuStates[i] = currentState;
            }
        }
    }

    void ToggleSecondaryMenu(int index, GameObject buttonGameObject)
    {
        StartCoroutine(AnimateMenuToggle(index, buttonGameObject));
        StartCoroutine(RotateButton1(openButton[index], 90f, 0.2f));
    }

    void ToggleSecondaryMenu(int index)
    {
        secondaryMenus[index].SetActive(!secondaryMenus[index].activeInHierarchy);
        StartCoroutine(RotateButton2(openButton[index], 90f, 0.2f));
    }

    IEnumerator AnimateMenuToggle(int index, GameObject buttonGameObject)
    {
        Button fourthButton = buttonGameObject.GetComponent<Button>();
        float animationDuration = 0.3f;

        // Toggle secondary menu visibility
        secondaryMenus[index].SetActive(!secondaryMenus[index].activeInHierarchy);
        isMenuOpen = !isMenuOpen;

        // Calculate new position for the fourth button
        Vector2 newPosition = isMenuOpen
            ? originalFourthButtonPosition + new Vector2(0, -450f)
            : originalFourthButtonPosition;

        // Animate the fourth button's position
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            fourthButtonRectTransform.anchoredPosition = Vector2.Lerp(
                fourthButtonRectTransform.anchoredPosition,
                newPosition,
                elapsed / animationDuration
            );
            elapsed += Time.deltaTime;
            yield return null;
        }
        fourthButtonRectTransform.anchoredPosition = newPosition;
    }

    IEnumerator RotateButton1(GameObject button, float targetAngle, float duration)
    {
        isRotated1 = !isRotated1;
        Quaternion startRotation = button.transform.localRotation;
        Quaternion endRotation = isRotated1
            ? Quaternion.Euler(0, 0, -targetAngle)
            : Quaternion.Euler(0, 0, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            button.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.transform.localRotation = endRotation;
    }

    IEnumerator RotateButton2(GameObject button, float targetAngle, float duration)
    {
        isRotated2 = !isRotated2;
        Quaternion startRotation = button.transform.localRotation;
        Quaternion endRotation = isRotated2
            ? Quaternion.Euler(0, 0, -targetAngle)
            : Quaternion.Euler(0, 0, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            button.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.transform.localRotation = endRotation;
    }

    IEnumerator SyncRotateButton(GameObject button, float targetAngle, float duration)
    {
        Quaternion startRotation = button.transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, targetAngle);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            button.transform.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.transform.localRotation = endRotation;
    }
}
