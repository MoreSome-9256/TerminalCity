using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QARecord
{
    public string questionText;   // 玩家提出的问题
    public List<(string speaker, string content)> Answers { get; set; } = new List<(string, string)>();
    public QARecord(string question)
    {
        questionText = question;
    }
    public QARecord() { }
}
