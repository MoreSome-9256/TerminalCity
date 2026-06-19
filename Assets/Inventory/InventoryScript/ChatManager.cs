using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class ChatManager : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject chatPanel;
    public TMP_Text chatContentText;
    public Button nextMessageButton;
    public Button closeChatButton;
    public ScrollRect chatScrollRect;

    [Header("对话设置")]
    [SerializeField] private string conversationEndMessage = "对话结束";

    private List<string> conversationChunks;
    private string conversationHeader;
    private int chunkIndex = 0;

    void Start()
    {
        conversationChunks = new List<string>();
        if (chatPanel != null) chatPanel.SetActive(false);
        if (nextMessageButton != null) nextMessageButton.onClick.AddListener(OnNextMessageClicked);
        if (closeChatButton != null) closeChatButton.onClick.AddListener(CloseChat);
    }

    /// <summary>
    /// 外部调用此方法，开始一段由符号控制的对话
    /// </summary>
    public void StartConversation(TextAsset conversationFile)
    {
        if (!ParseConversationFile(conversationFile))
        {
            Debug.LogError("解析对话文件失败！请确保文件包含 *start, *c, 和 *end 符号。");
            return;
        }

        // --- 核心修复点 ---
        // 不要直接激活面板，而是启动协程来做这件事
        nextMessageButton.interactable = true;
        StartCoroutine(ActivateChatPanel());
    }

    /// <summary>
    ///  --- 核心修复点 ---
    /// 这个协程是解决“首次点击无效”的关键
    /// </summary>
    private IEnumerator ActivateChatPanel()
    {
        // 1. 先在后台把所有东西都准备好
        chunkIndex = 0;
        chatContentText.text = conversationHeader;
        nextMessageButton.gameObject.SetActive(true);

        // 2. 等待当前帧的末尾。这给了EventSystem充足的准备时间。
        yield return new WaitForEndOfFrame();

        // 3. 在下一帧的开头，再安全地激活UI面板。此时EventSystem已经就绪。
        chatPanel.SetActive(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            chatPanel.GetComponent<RectTransform>()
        );
    }

    /// <summary>
    /// 核心显示逻辑：每次点击，显示下一个由*c分割的区块
    /// </summary>
    private void OnNextMessageClicked()
    {
        if (chunkIndex >= conversationChunks.Count)
        {
            chatContentText.text += $"\n\n--- {conversationEndMessage} ---";
            nextMessageButton.gameObject.SetActive(false);
            StartCoroutine(ForceScrollDown());
            return;
        }

        string chunkToShow = conversationChunks[chunkIndex];
        chatContentText.text += chunkToShow;
        chunkIndex++;
        StartCoroutine(ForceScrollDown());
    }

    /// <summary>
    /// 基于符号的解析方法，这个是正确的，保持不变
    /// </summary>
    private bool ParseConversationFile(TextAsset file)
    {
        if (file == null || string.IsNullOrEmpty(file.text)) return false;

        conversationChunks.Clear();
        string fullText = file.text;

        int startIndex = fullText.IndexOf("*start");
        int endIndex = fullText.IndexOf("*end");

        if (startIndex == -1 || endIndex == -1) return false;

        conversationHeader = fullText.Substring(0, startIndex).Trim();
        int bodyStartIndex = startIndex + "*start".Length;
        string body = fullText.Substring(bodyStartIndex, endIndex - bodyStartIndex);

        string[] chunks = body.Split(new[] { "*c" }, System.StringSplitOptions.None);
        conversationChunks = chunks.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        return conversationChunks.Count > 0;
    }

    // --- 辅助方法，保持不变 ---
    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        if (chatScrollRect != null) chatScrollRect.verticalNormalizedPosition = 0f;
    }

    public void CloseChat()
    {
        if (chatPanel != null) chatPanel.SetActive(false);
    }
    public void DisplayFullConversation(TextAsset conversationFile)
    {
        if (conversationFile == null) return;

        // 解析对话文件
        if (!ParseConversationFile(conversationFile))
        {
            Debug.LogError("解析对话文件失败！");
            return;
        }

        // 直接把所有区块拼起来
        chatContentText.text = conversationHeader + string.Join("", conversationChunks);
        chatPanel.SetActive(true);
        nextMessageButton.gameObject.SetActive(false); // 不显示“下一条”按钮
        Canvas.ForceUpdateCanvases();
        if (chatScrollRect != null)
            chatScrollRect.verticalNormalizedPosition = 0f; // 滚动到底
    }

}
