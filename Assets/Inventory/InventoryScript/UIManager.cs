using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Inventory UI")]
    public GameObject previewArea;  // 对应 PreView 面板
    public GameObject firstLevel;   // 对应 FirstLevel 面板 / 背包 UI
    public GameObject doorInventoryUI;  // 门解锁专用的背包面板（若与 firstLevel 相同可拖同一个）
    public GameObject messagePanel;     // 对应 Message 提示面板
    public GameObject moveButton;       // 打开背包的侧边栏按钮（MoveButton）

    [Header("Dialogue UI")]
    public GameObject dialogueUI;
    public TMP_Text charNameText;
    public TMP_Text dialogueText;
    public Image characterImage;
    public Button nextButton;
    public GameObject dialogueBackground;

    [Header("Selection UI")]
    public GameObject selectionUI;

    [Header("Room Canvas")]
    [Tooltip("场景中全局渲染 Room 的 Canvas 物体或根容器")]
    public RectTransform roomPanel;

    [Header("Room Info UI")]
    public GameObject roomInfoPanel;
    public TMP_Text instabilityText;
    public TMP_Text descriptionText;
    public Image npcImage;

    [Header("Level3 UI")]
    public Transform viewPanelTransform;

    [Header("Big Room Edge Pan")]
    [Tooltip("挂载在全局 Canvas 上的大房间边缘平移控制器")]
    public RoomEdgePanController edgePanController;

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
