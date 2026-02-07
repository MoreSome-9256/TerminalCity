using System.Collections;
using UnityEngine;

public abstract class SequenceStep : ScriptableObject
{
    // root 是 prefab 根节点
    public abstract IEnumerator Play(Level3Context context, Transform root);
}
