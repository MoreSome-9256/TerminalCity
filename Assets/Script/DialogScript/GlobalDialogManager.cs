using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.Data.Sqlite;
using System.IO;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class GlobalDialogManager : MonoBehaviour
{
    public static GlobalDialogManager Instance { get; private set; }

    public string charName;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text charNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image characterImage;
    [SerializeField] private GameObject background;
    [SerializeField] private Button nextButton;
    [SerializeField] public GameObject selectionUI;
    //[SerializeField] private GameObject dialoguePanel;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.05f;

    private List<DialogueSegment> currentDialogue;
    private int currentIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private string dbPath;

    private bool isDialogueActive;                 // 新增：是否正在对话
    [SerializeField] private float advanceCooldown = 0.08f; // 新增：防抖冷却(秒)
    private float _lastAdvanceTime = -999f;       // 新增：上次触发时间

    [Header("Branch UI")]
    [SerializeField] private GameObject branchPanel;     // 包含按钮的父物体
    [SerializeField] private Button branchButtonPrefab;  // 预制按钮，用于生成分支按钮

    [Header("UI Control")]
    [SerializeField] private Button targetButton;
    private bool wasButtonInitiallyActive;
    private bool hasSearchedButton;

    // Q&A 历史记录
    public static List<QARecord> qaHistory = new List<QARecord>();
    // 临时记录：玩家刚刚问了什么问题
    private string pendingQuestion = null;
    // 已选择过的选项 flag
    public static List<string> triggeredFlags = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 数据库路径（StreamingAssets 下）
        dbPath = Path.Combine(Application.streamingAssetsPath, "dialog_global.db");

        // 初始化 UI 关闭
        //dialoguePanel.SetActive(false);
        dialogueUI.SetActive(false);
        charNameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        characterImage?.gameObject.SetActive(false);
        if (background != null)
        {
            background.SetActive(false);
        }
        isDialogueActive = false;
    }
    private void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 如果当前 EventSystem 选中的是 nextButton，空格会触发 onClick，
            // 此时我们不在 Update 再触发一次，避免双击。
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != nextButton.gameObject)
            {
                TryAdvance();
            }
        }
    }

    public void StartDialogue(string dialogueID)
    {
        bool hasButton = TryFindButton();
        if (hasButton)
        {
            wasButtonInitiallyActive = targetButton.interactable;
            targetButton.interactable = false;
            targetButton.gameObject.SetActive(false);
        }
        currentDialogue = LoadDialogueFromDB(dialogueID);
        if (currentDialogue == null || currentDialogue.Count == 0)
        {
            Debug.LogWarning($"[GlobalDialogManager] Dialogue '{dialogueID}' not found in DB.");
            return;
        }

        currentIndex = 0;
        selectionUI.SetActive(false);
        dialogueUI.SetActive(true);
        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        characterImage?.gameObject.SetActive(true);
        if (background != null) background.SetActive(true);

        ShowDialogueSegment(currentDialogue[currentIndex]);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(TryAdvance);   //改：按钮走 TryAdvance

        isDialogueActive = true;                      //标记激活
        _lastAdvanceTime = -999f;                     //重置冷却

        // 可选：避免按钮被选中从而空格触发 Submit → 双触发
        EventSystem.current?.SetSelectedGameObject(null);
    }
    public void ShowDialogueByID(int id, Action onComplete = null)
    {
        // 读取单条对话
        DialogueSegment segment = LoadDialogueSegmentFromDB(id);
        if (segment == null)
        {
            Debug.LogWarning($"[GlobalDialogManager] Dialogue ID {id} not found.");
            onComplete?.Invoke();
            return;
        }

        // 显示 UI
        selectionUI.SetActive(false);
        dialogueUI.SetActive(true);
        charNameText.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
        characterImage?.gameObject.SetActive(true);
        if (background != null) background.SetActive(true);

        // 停止已有打字效果
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        // 开启打字效果
        typingCoroutine = StartCoroutine(TypeText(segment.dialogueText, () =>
        {
            // 台词打完后触发回调
            onComplete?.Invoke();
        }));

        isDialogueActive = true;
        _lastAdvanceTime = -999f;

        // 可选：避免按钮被选中从而空格触发 Submit → 双触发
        EventSystem.current?.SetSelectedGameObject(null);
    }
    // 统一入口 + 防抖
    private void TryAdvance()
    {
        if (!isDialogueActive) return;  // 结束对话后不再触发
        if (Time.unscaledTime - _lastAdvanceTime < advanceCooldown) return;

        _lastAdvanceTime = Time.unscaledTime;
        NextDialogue();
    }

    private List<DialogueSegment> LoadDialogueFromDB(string startID)
    {
        List<DialogueSegment> dialogueList = new List<DialogueSegment>();

        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            string currentID = startID;

            while (!string.IsNullOrEmpty(currentID))
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, speaker, text, spritePath, backgroundPath, nextID, branchGroup, conditionFlag FROM dialogue WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", currentID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DialogueSegment segment = new DialogueSegment();
                            segment.id = reader.GetInt32(0);              // id
                            segment.charName = reader.GetString(1);       // speaker
                            segment.dialogueText = reader.GetString(2);   // text

                            string spritePath = reader.IsDBNull(3) ? null : reader.GetString(3);
                            if (!string.IsNullOrEmpty(spritePath))
                                segment.characterSprite = Resources.Load<Sprite>(spritePath);

                            string bgPath = reader.IsDBNull(4) ? null : reader.GetString(4);
                            if (!string.IsNullOrEmpty(bgPath))
                                segment.backgroundSprite = Resources.Load<Sprite>(bgPath);

                            string nextID = reader.IsDBNull(5) ? null : reader.GetInt32(5).ToString();
                            // 如果下个ID是 "-1" 或者小于0的值，直接视为结束，不再继续查询
                            if (nextID == "-1" || string.IsNullOrEmpty(nextID))
                            {
                                currentID = null;
                            }
                            else
                            {
                                currentID = nextID;
                            }
                            segment.branchGroupID = reader.IsDBNull(6) ? null : reader.GetString(6);
                            segment.conditionFlag = reader.IsDBNull(7) ? null : reader.GetString(7);

                            dialogueList.Add(segment);

                            // 准备下一轮
                            currentID = nextID;
                        }
                        else
                        {
                            break; // 查不到就退出
                        }
                    }
                }
            }
        }

        return dialogueList;
    }

    private void ShowDialogueSegment(DialogueSegment segment)
    {
        // 🔍 核心修改：将原本的 segment.charName 和 dialogueText 视为 Key 进行翻译
        string localizedName = GetLocalizedText($"char_{segment.charName}", segment.charName);
        string localizedText = GetLocalizedText($"dialog_{segment.id}", segment.dialogueText);

        // Set UI
        charNameText.text = localizedName; //
        characterImage.sprite = segment.characterSprite; //[cite: 1]

        if (background != null)
        {
            background.SetActive(true); //[cite: 1]
        }

        // Start typing effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine); //[cite: 1]

        // 🔍 传入翻译后的台词，而不是原数据库里的字
        typingCoroutine = StartCoroutine(TypeText(localizedText));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
    // 修改打字协程，增加完成回调
    private IEnumerator TypeText(string fullText, Action onComplete)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in fullText)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        onComplete?.Invoke();
    }

    private void NextDialogue()
    {
        if (currentDialogue == null || currentIndex < 0 || currentIndex >= currentDialogue.Count)
            return;

        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine); //[cite: 1]

            // 🔍 核心修改：这里原本是直接写 segment.dialogueText，改成获取翻译后的文本
            string localizedText = GetLocalizedText($"dialog_{currentDialogue[currentIndex].id}", currentDialogue[currentIndex].dialogueText);
            dialogueText.text = localizedText;

            isTyping = false; //[cite: 1]
            return; //[cite: 1]
        }

        // 如果是问答的第一句回答，创建 QARecord
        if (pendingQuestion != null && currentIndex == 0)
        {
            var record = new QARecord(pendingQuestion);
            GlobalDialogManager.qaHistory.Add(record);
        }

        // 只有当 pendingQuestion 不为空时，才写入回答
        if (pendingQuestion != null)
        {
            QARecord currentRecord = GlobalDialogManager.qaHistory.Count > 0
                ? GlobalDialogManager.qaHistory[GlobalDialogManager.qaHistory.Count - 1]
                : null;

            if (currentRecord != null)
            {
                var segment = currentDialogue[currentIndex];
                currentRecord.Answers.Add((segment.charName, segment.dialogueText));
            }
        }

        currentIndex++;

        if (currentIndex >= currentDialogue.Count)
        {
            var lastSegment = currentDialogue[currentDialogue.Count - 1];
            var groupId = lastSegment.branchGroupID;

            // 问答结束，清空 pendingQuestion
            pendingQuestion = null;

            if (!string.IsNullOrEmpty(groupId) && HasBranchOptions(groupId))
            {
                nextButton.gameObject.SetActive(false);
                ShowBranchOptions(groupId);
            }
            else
            {
                EndDialogue();
            }
            return;
        }

        ShowDialogueSegment(currentDialogue[currentIndex]);
    }

    private void ShowBranchOptions(string branchGroupID)
    {
        foreach (Transform t in branchPanel.transform)
            Destroy(t.gameObject);

        List<BranchOption> options = LoadBranchOptionsFromDB(branchGroupID);

        if (options == null || options.Count == 0)
        {
            branchPanel.SetActive(false);
            EndDialogue();
            return;
        }

        branchPanel.SetActive(true);

        bool hasStoryBranch = options.Any(opt => opt.isStoryBranch); // 检查是否剧情分支

        int shownCount = 0;
        foreach (var opt in options)
        {
            var localOpt = opt;

            if (!string.IsNullOrEmpty(localOpt.conditionFlag) &&
                GlobalDialogManager.triggeredFlags.Contains(localOpt.conditionFlag))
            {
                continue;
            }

            if (shownCount >= 3) break;

            Button btn = Instantiate(branchButtonPrefab, branchPanel.transform);
            string localizedOption = GetLocalizedText($"branch_{branchGroupID}_{shownCount}", localOpt.optionText);
            btn.GetComponentInChildren<TMP_Text>().text = localOpt.optionText;

            btn.onClick.AddListener(() =>
            {
                pendingQuestion = localizedOption;

                if (!string.IsNullOrEmpty(localOpt.conditionFlag))
                {
                    GlobalDialogManager.triggeredFlags.Add(localOpt.conditionFlag);
                }

                if (targetButton != null && wasButtonInitiallyActive)
                {
                    targetButton.interactable = true;
                    targetButton.gameObject.SetActive(true);
                }

                branchPanel.SetActive(false);
                TriggerDialogue(localOpt.targetDialogueID);
            });

            shownCount++;
        }

        // 只有非剧情分支才额外加“随便聊聊”和“没什么事了”
        if (!hasStoryBranch)
        {
            // 随便聊聊
            Button chatBtn = Instantiate(branchButtonPrefab, branchPanel.transform);
            chatBtn.GetComponentInChildren<TMP_Text>().text = GetLocalizedText("ui_chat_casual", "随便聊聊");
            chatBtn.onClick.AddListener(() =>
            {
                branchPanel.SetActive(false);
                if (targetButton != null && wasButtonInitiallyActive)
                {
                    targetButton.interactable = true;
                    targetButton.gameObject.SetActive(true);
                }
                TriggerRandomCasualDialogue(charName);
            });

            // 没什么事了
            Button exitBtn = Instantiate(branchButtonPrefab, branchPanel.transform);
            exitBtn.GetComponentInChildren<TMP_Text>().text = GetLocalizedText("ui_chat_exit", "没什么事了");
            exitBtn.onClick.AddListener(() =>
            {
                branchPanel.SetActive(false);
                DialogueSegment segment = LoadDialogueSegmentFromDB(999);
                if (segment != null)
                {
                    currentDialogue = new List<DialogueSegment> { segment };
                    currentIndex = 0;
                    ShowDialogueSegment(currentDialogue[currentIndex]);
                    nextButton.gameObject.SetActive(true);
                    isDialogueActive = true;
                    _lastAdvanceTime = -999f;
                    EventSystem.current?.SetSelectedGameObject(null);
                }
                else
                {
                    EndDialogue();
                }
            });
        }
    }

    private bool HasBranchOptions(string branchGroupID)
    {
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(1) FROM branch WHERE branchGroup=@id";
                cmd.Parameters.AddWithValue("@id", branchGroupID);
                int count = System.Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }


    private void EndDialogue()
    {
        if (currentDialogue != null && currentDialogue.Count > 0)
        {
            var lastSegment = currentDialogue[currentDialogue.Count - 1];
            if (!string.IsNullOrEmpty(lastSegment.conditionFlag) &&
                lastSegment.conditionFlag.ToLower() == "trigger")
            {
                DialogueEventDatabase eventDB = FindObjectOfType<DialogueEventDatabase>();
                if (eventDB != null)
                {
                    eventDB.TriggerEvents(lastSegment.id);
                }
            }
        }
        if (targetButton != null && wasButtonInitiallyActive)
        {
            targetButton.interactable = true;
            targetButton.gameObject.SetActive(true);
        }
        charNameText.text = string.Empty;
        dialogueText.text = string.Empty;
        dialogueUI.SetActive(false);
        charNameText.gameObject.SetActive(false);
        dialogueText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        characterImage?.gameObject.SetActive(false);
        if (background != null) background.SetActive(false);

        isDialogueActive = false; // 结束标记

        if (RoomManager.Instance != null && RoomManager.Instance.currentRoomID == 0)
        {
            if (selectionUI != null)
                selectionUI.SetActive(true);
        }
    }

    [System.Serializable]
    public class DialogueSegment
    {
        public int id;
        public string charName;
        public string dialogueText;
        public Sprite characterSprite;
        public Sprite backgroundSprite;
        public string branchGroupID;
        public string conditionFlag;
    }
    private List<BranchOption> LoadBranchOptionsFromDB(string branchGroupID)
    {
        List<BranchOption> list = new List<BranchOption>();
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT optionText, nextID, conditionFlag, isStoryBranch FROM branch WHERE branchGroup=@id ORDER BY rowid ASC";
                cmd.Parameters.AddWithValue("@id", branchGroupID);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BranchOption
                        {
                            optionText = reader.GetString(0),
                            targetDialogueID = reader.GetInt32(1).ToString(),
                            conditionFlag = reader.IsDBNull(2) ? null : reader.GetString(2),
                            isStoryBranch = !reader.IsDBNull(3) && reader.GetString(3).ToLower() == "true"  // 👈 sqlite text 转 bool
                        });
                    }
                }
            }
        }
        return list;
    }

    // 读取单条对话
    private DialogueSegment LoadDialogueSegmentFromDB(int id)
    {
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, speaker, text, spritePath, backgroundPath, conditionFlag FROM dialogue WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DialogueSegment segment = new DialogueSegment();
                        segment.id = reader.GetInt32(0);
                        segment.charName = reader.GetString(1);
                        segment.dialogueText = reader.GetString(2);

                        string spritePath = reader.IsDBNull(3) ? null : reader.GetString(3);
                        if (!string.IsNullOrEmpty(spritePath))
                            segment.characterSprite = Resources.Load<Sprite>(spritePath);

                        string bgPath = reader.IsDBNull(4) ? null : reader.GetString(4);
                        if (!string.IsNullOrEmpty(bgPath))
                            segment.backgroundSprite = Resources.Load<Sprite>(bgPath);
                        segment.conditionFlag = reader.IsDBNull(7) ? null : reader.GetString(7);

                        return segment;
                    }
                }
            }
        }
        return null;
    }

    [System.Serializable]
    public class BranchOption
    {
        public string optionText;
        public string targetDialogueID;
        public string conditionFlag;
        public bool isStoryBranch;
    }

    public void TriggerDialogue(string dialogueID)
    {
        SidePanelManager.Instance.HideAll();
        StartDialogue(dialogueID);
    }

    // 随机挑选一条开场语（比如洛的问候语）
    public void TriggerRandomOpening(List<int> candidateIDs)
    {
        List<int> validIDs = new List<int>();

        foreach (int id in candidateIDs)
        {
            DialogueSegment seg = LoadDialogueSegmentFromDB(id);
            if (seg == null) continue;

            // 检查条件
            if (CheckCondition(seg.id))
            {
                validIDs.Add(id);
            }
        }

        if (validIDs.Count == 0)
        {
            Debug.LogWarning("没有符合条件的开场语！");
            return;
        }

        // 随机选一条
        int chosenID = validIDs[UnityEngine.Random.Range(0, validIDs.Count)];

        // 直接走原有逻辑
        TriggerDialogue(chosenID.ToString());
    }

    // 根据 conditionFlag 检查是否满足条件
    private bool CheckCondition(int dialogueID)
    {
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT conditionFlag FROM dialogue WHERE id = @id";
                cmd.Parameters.AddWithValue("@id", dialogueID);

                var flag = cmd.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(flag) || flag == "always")
                    return true;

                if (flag == "firstOnly")
                    return qaHistory.Count == 0;

                if (flag == "notFirst")
                    return qaHistory.Count > 0;

                /*if (flag.StartsWith("docs>"))
                {
                    int required = int.Parse(flag.Substring(5));
                    return GameState.collectedDocsCount > required;
                }*/

                // 其他条件扩展
                return false;
            }
        }
    }
    public void TriggerRandomCasualDialogue(string category)
    {
        List<int> candidateIDs = LoadCasualDialogueIDs(category);
        List<int> validIDs = new List<int>();

        foreach (int id in candidateIDs)
        {
            if (CheckCondition(id)) validIDs.Add(id);
        }

        if (validIDs.Count == 0)
        {
            Debug.Log($"没有符合条件的随便聊聊对话（角色 {category}）");
            EndDialogue();
            return;
        }

        string chosenID = validIDs[UnityEngine.Random.Range(0, validIDs.Count)].ToString();

        TriggerDialogue(chosenID);
    }

    private List<int> LoadCasualDialogueIDs(string category)
    {
        List<int> ids = new List<int>();
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id FROM dialogue WHERE type='casual' AND category=@cat";
                cmd.Parameters.AddWithValue("@cat", category);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt32(0));
                    }
                }
            }
        }
        return ids;
    }
    private bool TryFindButton()
    {
        if (targetButton != null) return true;
        if (hasSearchedButton) return false; // 避免重复查找

        // 尝试按路径查找
        Transform buttonTransform = GameObject.Find("GlobalUI")?
                                    .transform.Find("Canvas/sideScreen/Button");

        if (buttonTransform != null)
        {
            targetButton = buttonTransform.GetComponent<Button>();
            //Debug.Log("自动查找到按钮: " + targetButton.name);
        }

        hasSearchedButton = true;
        return targetButton != null;
    }

    /// <summary>
    /// 本地化文本获取中心（未来的多语言安全锁）
    /// </summary>
    /// <param name="key">推荐给该文本定义的唯一 Key</param>
    /// <param name="fallbackValue">如果找不到翻译，或者目前还没做翻译时返回的默认文本（即你目前的中文）</param>
    private string GetLocalizedText(string key, string fallbackValue)
    {
        // ==========================================
        // 以后接入 Unity Localization 时，只需解开这里的注释：
        // try {
        //     // 假设你的本地化表名叫 "DialogueTable"
        //     string translated = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString("DialogueTable", key);
        //     if (!string.IsNullOrEmpty(translated)) return translated;
        // } catch { 
        //     /* 预防未找到 Key 报错 */ 
        // }
        // ==========================================

        // 目前阶段：直接返回原本的中文内容，完全不影响你现在的测试和开发
        return fallbackValue;
    }
}
