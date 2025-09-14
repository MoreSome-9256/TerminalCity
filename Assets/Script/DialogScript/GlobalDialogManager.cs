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
    [SerializeField] private GameObject selectionUI;
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
                    cmd.CommandText = "SELECT id, speaker, text, spritePath, backgroundPath, nextID, branchGroup FROM dialogue WHERE id = @id";
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
                            segment.branchGroupID = reader.IsDBNull(6) ? null : reader.GetString(6);

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
        // Set UI
        charNameText.text = segment.charName;
        characterImage.sprite = segment.characterSprite;

        if (background != null)
        {
            background.SetActive(true);
        }

        // Start typing effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(segment.dialogueText));
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
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            dialogueText.text = currentDialogue[currentIndex].dialogueText;
            isTyping = false;
            return;
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
        // 清空旧按钮
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

        int shownCount = 0;
        foreach (var opt in options)
        {
            var localOpt = opt;

            // 跳过已触发
            if (!string.IsNullOrEmpty(localOpt.conditionFlag) &&
                GlobalDialogManager.triggeredFlags.Contains(localOpt.conditionFlag))
            {
                continue;
            }

            // 如果已经显示够 3 个，停止
            if (shownCount >= 3) break;

            // 生成按钮
            Button btn = Instantiate(branchButtonPrefab, branchPanel.transform);
            btn.GetComponentInChildren<TMP_Text>().text = localOpt.optionText;

            btn.onClick.AddListener(() =>
            {
                pendingQuestion = localOpt.optionText;

                if (!string.IsNullOrEmpty(localOpt.conditionFlag))
                {
                    GlobalDialogManager.triggeredFlags.Add(localOpt.conditionFlag);
                    Debug.Log("Added flag: " + localOpt.conditionFlag + ", now count: " + GlobalDialogManager.triggeredFlags.Count);
                }
                // 恢复按钮（上一段结束）
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
        // 固定随便聊聊按钮
        Button chatBtn = Instantiate(branchButtonPrefab, branchPanel.transform);
        chatBtn.GetComponentInChildren<TMP_Text>().text = "随便聊聊";
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
        // 固定退出按钮
        Button exitBtn = Instantiate(branchButtonPrefab, branchPanel.transform);
        exitBtn.GetComponentInChildren<TMP_Text>().text = "没什么事了";
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

                // 退出对话也算 QARecord
                if (pendingQuestion != null)
                {
                    var record = new QARecord
                    {
                        questionText = pendingQuestion
                    };
                    record.Answers.Add(("系统", segment.dialogueText));
                    GlobalDialogManager.qaHistory.Add(record);

                    pendingQuestion = null;
                }
            }
            else
            {
                EndDialogue();
            }
        });
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
    }
    private List<BranchOption> LoadBranchOptionsFromDB(string branchGroupID)
    {
        List<BranchOption> list = new List<BranchOption>();
        using (var conn = new SqliteConnection($"URI=file:{dbPath}"))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                // 使用实际列名 nextID
                cmd.CommandText = "SELECT optionText, nextID, conditionFlag FROM branch WHERE branchGroup=@id ORDER BY rowid ASC";
                cmd.Parameters.AddWithValue("@id", branchGroupID);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BranchOption
                        {
                            optionText = reader.GetString(0),
                            targetDialogueID = reader.GetInt32(1).ToString(), // INTEGER 转 string
                            conditionFlag = reader.GetString(0)
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
                cmd.CommandText = "SELECT id, speaker, text, spritePath, backgroundPath FROM dialogue WHERE id = @id";
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
    }

    public void TriggerDialogue(string dialogueID)
    {
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
}
