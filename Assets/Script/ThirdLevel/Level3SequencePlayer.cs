using UnityEngine;

public static class Level3SequencePlayer
{
    public static void Play(Level3Data data)
    {
        if (data == null || data.sequencePrefab == null)
        {
            Debug.LogWarning("没有配置 sequencePrefab");
            return;
        }

        Transform parent = UIManager.Instance.viewPanelTransform;
        GameObject go = GameObject.Instantiate(
            data.sequencePrefab,
            parent,
            false
        );

        Level3SequenceController controller =
            go.GetComponent<Level3SequenceController>();

        if (controller != null)
        {
            controller.Play();   // ← 改成无参
        }
        else
        {
            Debug.LogWarning("sequencePrefab 上没有 Level3SequenceController");
        }
    }
}
