using System.Collections;
using UnityEngine;

public class RoomShake : MonoBehaviour
{
    [Header("抖动参数")]
    [Tooltip("抖动持续时间 (秒)")]
    public float duration = 0.35f;

    [Tooltip("抖动最大幅度 (UI建议设大一点，比如 20~35 像素以便观察效果)")]
    public float magnitude = 25f;

    [Tooltip("抖动频率")]
    public float frequency = 50f;

    [Tooltip("是否随时间平滑衰减归零")]
    public bool decayOverTime = true;

    [Header("抖动轴向")]
    public bool shakeX = true;
    public bool shakeY = true;

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Vector3 originalLocalPos;
    private Coroutine shakeCoroutine;
    private bool isUI = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        isUI = rectTransform != null;
        RecordBasePosition();
    }

    private void RecordBasePosition()
    {
        if (isUI && rectTransform != null)
        {
            originalAnchoredPos = rectTransform.anchoredPosition;
        }
        else
        {
            originalLocalPos = transform.localPosition;
        }
    }

    public void TriggerShake()
    {
        TriggerShake(duration, magnitude, frequency);
    }

    public void TriggerShake(float customDuration, float customMagnitude, float customFrequency)
    {
        Debug.Log($"[RoomShake] 执行抖动: {gameObject.name}，幅度: {customMagnitude}");

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            ResetPosition();
        }
        else
        {
            RecordBasePosition();
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(customDuration, customMagnitude, customFrequency));
    }

    private IEnumerator ShakeRoutine(float totalDuration, float maxMagnitude, float freq)
    {
        float elapsed = 0f;
        float seedX = Random.Range(0f, 100f);
        float seedY = Random.Range(100f, 200f);

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float percent = Mathf.Clamp01(elapsed / totalDuration);
            float damper = decayOverTime ? (1f - percent) : 1f;

            float timeParam = elapsed * freq;
            float offsetX = shakeX ? (Mathf.PerlinNoise(seedX, timeParam) * 2f - 1f) * maxMagnitude * damper : 0f;
            float offsetY = shakeY ? (Mathf.PerlinNoise(seedY, timeParam) * 2f - 1f) * maxMagnitude * damper : 0f;

            if (isUI)
            {
                rectTransform.anchoredPosition = originalAnchoredPos + new Vector2(offsetX, offsetY);
            }
            else
            {
                transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
            }

            yield return null;
        }

        ResetPosition();
        shakeCoroutine = null;
    }

    public void ResetPosition()
    {
        if (isUI && rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPos;
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        ResetPosition();
    }

    private void OnDisable()
    {
        StopShake();
    }
}