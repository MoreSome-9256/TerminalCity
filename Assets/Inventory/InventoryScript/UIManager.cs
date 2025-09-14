using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Inventory UI")]
    public GameObject previewArea;  // 对应 PreView 面板
    public GameObject firstLevel;   // 对应 FirstLevel 面板 / 背包 UI

    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    public TMP_Text charNameText;
    public TMP_Text dialogueText;
    public Image characterImage;
    public Button nextButton;
    public GameObject dialogueBackground;

    [Header("Selection UI")]
    public GameObject selectionUI;

    [Header("Room Info UI")]
    public GameObject roomInfoPanel;
    public TMP_Text instabilityText;
    public TMP_Text descriptionText;
    public Image npcImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 如果需要跨场景
    }
}
