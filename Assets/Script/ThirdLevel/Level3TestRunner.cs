using UnityEngine;

public class Level3TestRunner : MonoBehaviour
{
    [SerializeField] private GameObject sequencePrefab; // 拖 Project 文件夹里的 Opening Prefab
    [SerializeField] private Transform parentPanel;     // 拖场景里的 ViewPanel

    void Start()
    {
        if (sequencePrefab == null)
        {
            Debug.LogError("Level3TestRunner: sequencePrefab not assigned!");
            return;
        }

        // 1. 动态生成到 ViewPanel 下
        GameObject go = Instantiate(sequencePrefab, parentPanel != null ? parentPanel : transform);
        go.SetActive(true);

        // 2. 获取组件并播放
        Level3SequenceController controller = go.GetComponent<Level3SequenceController>();
        if (controller != null)
        {
            controller.Play();
        }
    }
}