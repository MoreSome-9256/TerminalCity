using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QAHistoryPanel : MonoBehaviour
{
    public Transform contentParent;      // ScrollView 下的 Content
    public GameObject qaItemPrefab;      // 每条记录的预制体
    public GameObject answerItemPrefab;  // 每句回答的小 prefab

    private void OnEnable()
    {
        RefreshQAHistoryUI();
    }

    public void RefreshQAHistoryUI()
    {
        //Debug.Log("RefreshQAHistoryUI()");
        // 清空旧内容
        foreach (Transform t in contentParent)
            Destroy(t.gameObject);

        // 遍历历史记录
        foreach (var record in GlobalDialogManager.qaHistory)
        {
            // 实例化 QAItem
            GameObject item = Instantiate(qaItemPrefab, contentParent);
            Debug.Log("QA Record Question: " + record.questionText + " | QAItem children: " + item.transform.childCount);

            // 找到问题文本和回答容器
            TMP_Text questionTMP = item.transform.Find("Question").GetComponent<TMP_Text>();
            Transform answerContainer = item.transform.Find("AnswerContainer");

            questionTMP.text = record.questionText;

            // 清空旧回答（防止重用 prefab 时残留）
            foreach (Transform t in answerContainer)
                Destroy(t.gameObject);

            // 遍历回答列表，生成小 prefab
            foreach (var ans in record.Answers)
            {
                // answerItemPrefab 本身就是 TMP_Text
                GameObject answerItem = Instantiate(answerItemPrefab, answerContainer);
                TMP_Text ansTMP = answerItem.GetComponent<TMP_Text>();
                if (ansTMP != null)
                    ansTMP.text = $"{ans.speaker}：{ans.content}";
            }
        }
    }
}
