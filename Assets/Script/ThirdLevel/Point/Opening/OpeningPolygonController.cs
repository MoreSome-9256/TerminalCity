using UnityEngine;

public class OpeningPolygonController : MonoBehaviour
{
    [SerializeField] private InscribedPolygonRenderer polygonRenderer;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private int baseSides = 4;
    [SerializeField] private int stepSides = 2;

    private void Awake()
    {
        if (polygonRenderer == null)
            polygonRenderer = GetComponent<InscribedPolygonRenderer>();

        // 自动兜底：如果在 Inspector 没拖，去根节点下自动搜 DialogueManager
        if (dialogueManager == null)
            dialogueManager = GetComponentInParent<Level3SequenceController>()?.GetComponentInChildren<DialogueManager>();
    }

    private void OnEnable()
    {
        // 如果依然为空，全局兜底找一次
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null)
        {
            dialogueManager.onDialogueStep.RemoveListener(OnDialogueStep); // 防重
            dialogueManager.onDialogueStep.AddListener(OnDialogueStep);
            Debug.Log("<color=green>[OpeningPolygonController]</color> 成功注册监听 DialogueManager！");
        }
        else
        {
            Debug.LogError("<color=red>[OpeningPolygonController]</color> 未找到 DialogueManager，事件无法绑定！");
        }
    }

    private void OnDisable()
    {
        if (dialogueManager != null)
        {
            dialogueManager.onDialogueStep.RemoveListener(OnDialogueStep);
        }
    }

    private void Start()
    {
        // 初始为第 0 步
        UpdateSides(0);
    }

    public void OnDialogueStep(int index)
    {
        UpdateSides(index);
    }

    private void UpdateSides(int index)
    {
        int newSides = baseSides + index * stepSides;
        Debug.Log($"<color=cyan>[OpeningPolygonController 接收成功]</color> Index = {index}, 新边数 = {newSides}");

        if (polygonRenderer != null)
        {
            polygonRenderer.Sides = newSides;
        }
    }
}