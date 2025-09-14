using UnityEngine;
using UnityEngine.UI;

public class ChaosEffectManager : MonoBehaviour
{
    [Header("特效层 Panel")]
    public Image screenTearing;
    public Image faultEffect;
    public Image lightEffectB;
    public Image lightEffectR;

    [Header("LightEffectB Panel (挂Animator)")]
    public Animator lightEffectBAnimator;

    private string currentState = "";

    [Header("ScreenTearing Sprites")]
    public Sprite screenTearing1;
    public Sprite screenTearing2;

    [Header("FaultEffect Panel (挂Animator)")]
    public Animator faultEffectBAnimator;

    private string currentState2 = "";

    [Header("LightEffectR Colors")]
    public Color lightR1 = new Color(1, 0, 0, 0.2f);
    public Color lightR2 = new Color(1, 0, 0, 0.5f);

    private float faultTimer = 0f;
    private int faultIndex = 0;

    private void Update()
    {
        if (PlayerChaos.Instance == null) return;
        float chaos = PlayerChaos.Instance.chaos;
        if (chaos < 0.7f)
        {
            screenTearing.gameObject.SetActive(false);
            faultEffect.gameObject.SetActive(false);
            lightEffectB.gameObject.SetActive(false);
            lightEffectR.gameObject.SetActive(false);
        }
        // ----------------- ScreenTearing -----------------
        if (chaos >= 0.7f && chaos < 0.8f)
        {
            screenTearing.gameObject.SetActive(true);
            screenTearing.sprite = screenTearing1;
        }
        else if (chaos >= 0.8f) 
        {
            screenTearing.gameObject.SetActive(true);
            screenTearing.sprite = screenTearing2; 
        }

        // ----------------- LightEffectB -----------------
        if (chaos >= 0.7f && chaos < 0.8f)
        {
            PlayLightEffect("LightEffect1");
        }
        else if (chaos >= 0.8f)
        {
            PlayLightEffect("LightEffect2");
        }

        // ----------------- FaultEffect -----------------
        if (chaos >= 0.9f && chaos < 1.0f)
        {
            PlayFaultEffect("FaultEffect1");
        }else if(chaos >= 1.0f)
        {
            PlayFaultEffect("FaultEffect2");
        }

        // ----------------- LightEffectR -----------------
        lightEffectR.gameObject.SetActive(chaos >= 0.9f);
        if (chaos >= 0.9f) lightEffectR.color = (chaos < 1f) ? lightR1 : lightR2;
    }

    // ----------------- LightEffectB 忽明忽暗 -----------------
    private void PlayLightEffect(string stateName)
    {
        if (currentState == stateName) return; // 已经在播放该动画
        lightEffectB.gameObject.SetActive(true);
        lightEffectBAnimator.Play(stateName);
        currentState = stateName;
    }

    // ----------------- FaultEffect 动画 -----------------
    private void PlayFaultEffect(string stateName)
    {
        if (currentState2 == stateName) return; // 已经在播放该动画
        faultEffect.gameObject.SetActive(true);
        faultEffectBAnimator.Play(stateName);
        currentState2 = stateName;
    }
}
