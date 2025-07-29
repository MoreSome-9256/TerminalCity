using UnityEngine;
using UnityEngine.UI;

public class FadeInController : MonoBehaviour
{
    public Animator animator; // 引用 Animator 组件
    public Image fadeIn;
    public string animName;

    void Start()
    {
        // 播放动画
        animator.Play(animName);
    }
    public void Disable()
    {
        fadeIn.enabled = false;
    }
}