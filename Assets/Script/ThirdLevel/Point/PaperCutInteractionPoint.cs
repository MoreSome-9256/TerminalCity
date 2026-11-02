using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PaperCutInteractionPoint : PlotPoint
{
    [Header("UI 容器与元素")]
    public GameObject interactionRoot;
    public RectTransform paperRect;
    public RectTransform knifeRect;
    [Tooltip("指定小刀的刀尖位置（作为划痕生成的判定点）")]
    public Transform knifeTip;               // <--- 新增：刀尖判定点
    public Transform scratchContainer;
    public GameObject scratchPrefab;

    [Header("划痕设置")]
    public int requiredScratchCount = 5;     // 需要划满的次数
    public float minDragDistance = 120f;     // 单次有效划痕所需的最小像素距离
    public float minTimeBetweenCuts = 0.25f; // 防连击判定冷却

    [Header("转场现实 CG")]
    public CanvasGroup realityCgGroup;       // 现实 CG 的 CanvasGroup
    public float realityFadeDuration = 1.0f; // 切换到现实的淡入时间
    public float realityHoldDuration = 1.5f; // 现实 CG 停留时间

    [Header("音频设置 - 刀划音效")]
    public AudioSource cutAudioSource;       // 用于播放划动声音的 AudioSource
    public AudioClip paperCutClip;           // 纸张撕裂声
    public AudioClip fleshCutClip;           // 切割血肉声
    [Range(0f, 1f)] public float minPitch = 0.9f;
    [Range(0f, 1.5f)] public float maxPitch = 1.1f;
    [Header("音效衰减曲线控制")]
    [Tooltip("血肉声开始出现时的进度阈值（如0.3表示划过30%后才开始听得到微弱肉声）")]
    [Range(0f, 0.5f)] public float fleshStartThreshold = 0.25f;

    [Header("音频设置 - 液体滴落背景音")]
    public AudioSource dripLoopAudioSource;  // 循环播放液体滴落的 AudioSource
    public float maxDripVolume = 0.8f;       // 滴落音效的最大音量

    // 内部运行状态
    private Vector2 dragOffset;              // <--- 新增：保持抓取偏移，防止小刀瞬移吸附
    private Canvas parentCanvas;
    private Camera uiCamera;
    private int currentScratchCount = 0;
    private bool isInteracting = false;
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private float lastCutTime = 0f;

    [Header("切割手感参数")]
    [Tooltip("在容器内向右累计移动多少像素触发一刀")]
    public float requiredRightDistance = 70f;
    [Tooltip("允许的最大上下偏移斜率（超过这个斜率算垂直拖动，不触发）")]
    public float maxVerticalRatio = 0.8f;

    [Header("划痕视觉生成随机范围")]
    public Vector2 scratchWidthRange = new Vector2(160f, 240f);  // 划痕长度范围
    public Vector2 scratchHeightRange = new Vector2(6f, 12f);    // 划痕粗细范围
    public Vector2 scratchAngleRange = new Vector2(-6f, 6f);     // 微小的角度倾斜（度）
    [Header("划痕对齐设置")]
    [Tooltip("勾选后，所有划痕的X轴坐标固定为0（水平对齐），Y轴跟随刀的高度")]
    public bool alignScratchCenterX = true;

    private bool hasCutInCurrentStroke = false;
    private bool isTrackingStrokeInContainer = false;
    private Vector2 strokeEnterPos;

    // 状态变量调整
    private float strokeStartX;
    private float strokeStartY;
    private bool readyToCut = true; // 是否准备好向右划出一刀

    public override IEnumerator Execute()
    {
        currentScratchCount = 0;
        isInteracting = true;
        isDragging = false;

        // 初始化显示
        if (interactionRoot != null)
            interactionRoot.SetActive(true);

        if (realityCgGroup != null)
        {
            realityCgGroup.alpha = 0f;
            realityCgGroup.gameObject.SetActive(false);
        }

        // 初始化背景水滴音效
        if (dripLoopAudioSource != null)
        {
            dripLoopAudioSource.volume = 0f;
            dripLoopAudioSource.loop = true;
            dripLoopAudioSource.Play();
        }

        parentCanvas = paperRect.GetComponentInParent<Canvas>();
        uiCamera = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    ? parentCanvas.worldCamera
                    : null;

        // 等待玩家完成所有划痕
        while (currentScratchCount < requiredScratchCount)
        {
            HandleKnifeDragInput();
            yield return null;
        }

        isInteracting = false;

        // 达到目标后转场进入现实
        yield return SwitchToRealityTransition();
    }

    private void HandleKnifeDragInput()
    {
        if (!isInteracting) return;

        Vector2 mousePos = Input.mousePosition;

        // 1. 按下鼠标：记录抓取偏移量，不让刀瞬间吸附到鼠标中心
        if (Input.GetMouseButtonDown(0))
        {
            if (knifeRect != null)
            {
                // 判断鼠标是否点中了刀
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(knifeRect, mousePos, uiCamera, out Vector2 localKnifeClick))
                {
                    if (knifeRect.rect.Contains(localKnifeClick))
                    {
                        isDragging = true;
                        readyToCut = true;

                        // 计算拖拽偏移量：小刀当前局部坐标 - 鼠标在父节点下的局部坐标
                        if (knifeRect.parent is RectTransform parentRect)
                        {
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, uiCamera, out Vector2 localMouseInParent);
                            dragOffset = (Vector2)knifeRect.localPosition - localMouseInParent;
                        }

                        // 记录刀尖的初始位置作为起刀点
                        RecordTipStartPosition();
                    }
                }
            }
        }

        // 2. 拖拽跟随与刀尖划割检测
        if (isDragging && Input.GetMouseButton(0))
        {
            // 带着偏移量更新小刀位置
            if (knifeRect != null && knifeRect.parent is RectTransform parentRect)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mousePos, uiCamera, out Vector2 localMouseInParent))
                {
                    knifeRect.localPosition = localMouseInParent + dragOffset;
                }
            }

            // 核心判定：现在看的是【刀尖】的位置，而不是鼠标的位置
            if (scratchContainer is RectTransform containerRect)
            {
                // 获取刀尖在屏幕上的实际坐标（如果没绑 knifeTip 就退化为用小刀中心）
                Vector3 tipWorldPos = knifeTip != null ? knifeTip.position : knifeRect.position;
                Vector2 tipScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, tipWorldPos);

                // 将刀尖屏幕坐标转换为 ScratchContainer 下的局部坐标
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, tipScreenPos, uiCamera, out Vector2 tipPosInContainer))
                {
                    if (containerRect.rect.Contains(tipPosInContainer))
                    {
                        float deltaX = tipPosInContainer.x - strokeStartX;

                        // 刀尖往左退：蓄力重置
                        if (deltaX < 0)
                        {
                            strokeStartX = tipPosInContainer.x;
                            strokeStartY = tipPosInContainer.y;
                            readyToCut = true;
                        }
                        // 刀尖往右划：触发切割
                        else if (readyToCut && deltaX >= requiredRightDistance)
                        {
                            float deltaY = Mathf.Abs(tipPosInContainer.y - strokeStartY);

                            if ((deltaY / Mathf.Max(deltaX, 0.001f)) <= maxVerticalRatio)
                            {
                                if (Time.time - lastCutTime >= minTimeBetweenCuts)
                                {
                                    // 触发切割：传入刀尖此时的 Y 轴高度
                                    PerformCutAt(new Vector2(0f, tipPosInContainer.y));

                                    lastCutTime = Time.time;
                                    readyToCut = false;
                                }
                            }
                            else
                            {
                                strokeStartX = tipPosInContainer.x;
                                strokeStartY = tipPosInContainer.y;
                            }
                        }
                    }
                    else
                    {
                        // 刀尖离开纸张，更新起刀点
                        strokeStartX = tipPosInContainer.x;
                        strokeStartY = tipPosInContainer.y;
                        readyToCut = true;
                    }
                }
            }
        }

        // 3. 松开鼠标
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // 辅助方法：记录刀尖在容器内的坐标
    private void RecordTipStartPosition()
    {
        if (scratchContainer is RectTransform containerRect)
        {
            Vector3 tipWorldPos = knifeTip != null ? knifeTip.position : knifeRect.position;
            Vector2 tipScreenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, tipWorldPos);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, tipScreenPos, uiCamera, out Vector2 tipPosInContainer))
            {
                strokeStartX = tipPosInContainer.x;
                strokeStartY = tipPosInContainer.y;
            }
        }
    }

    private void PerformCutAt(Vector2 cutPosition)
    {
        currentScratchCount++;
        float progress = Mathf.Clamp01((float)currentScratchCount / requiredScratchCount);

        // 生成居中、美化后的划痕
        SpawnCenteredScratch(cutPosition);

        // 声音递进
        PlayDynamicCutSound(progress);

        if (dripLoopAudioSource != null)
        {
            dripLoopAudioSource.volume = Mathf.Lerp(0f, maxDripVolume, progress);
        }
    }

    private void SpawnCenteredScratch(Vector2 cutPosition)
    {
        if (scratchPrefab == null || scratchContainer == null) return;

        GameObject scratch = Instantiate(scratchPrefab, scratchContainer);
        RectTransform scratchRect = scratch.GetComponent<RectTransform>();

        // 随机长度与厚度
        float width = Random.Range(scratchWidthRange.x, scratchWidthRange.y);
        float height = Random.Range(scratchHeightRange.x, scratchHeightRange.y);
        float angle = Random.Range(scratchAngleRange.x, scratchAngleRange.y);

        // 【核心修改】
        // X轴：如果要求相同，就固定在 ScratchContainer 的水平中心 (0f)
        float finalX = alignScratchCenterX ? 0f : cutPosition.x;
        // Y轴：严格跟随玩家当前这刀所处的实际高度
        float finalY = cutPosition.y;

        scratchRect.localPosition = new Vector2(finalX, finalY);
        scratchRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        scratchRect.sizeDelta = new Vector2(width, height);
    }

    private void PlayDynamicCutSound(float progress)
    {
        if (cutAudioSource == null) return;

        cutAudioSource.pitch = Random.Range(minPitch, maxPitch);

        // 1. 纸张音量：前中期保持清脆，超过一半后迅速跌落至0
        // 使用平滑插值，最后阶段完全消失
        float paperVolume = Mathf.Clamp01((1.0f - progress) * 1.5f);

        // 2. 血肉音量：前 threshold 进度内严格为 0，之后沿二次方曲线非线性激增
        float fleshVolume = 0f;
        if (progress > fleshStartThreshold)
        {
            // 归一化到 [0, 1] 区间
            float fleshProgress = (progress - fleshStartThreshold) / (1.0f - fleshStartThreshold);
            // 二次方加剧效果（前缓后急）
            fleshVolume = Mathf.Clamp01(Mathf.Pow(fleshProgress, 1.8f));
        }

        // 播放混合音效（低于0.02视为完全静音不播）
        if (paperCutClip != null && paperVolume > 0.02f)
        {
            cutAudioSource.PlayOneShot(paperCutClip, paperVolume);
        }
        if (fleshCutClip != null && fleshVolume > 0.02f)
        {
            cutAudioSource.PlayOneShot(fleshCutClip, fleshVolume);
        }
    }

    private IEnumerator SwitchToRealityTransition()
    {
        // 停用小刀与纸交互
        if (knifeRect != null) knifeRect.gameObject.SetActive(false);

        // 淡入现实 CG
        if (realityCgGroup != null)
        {
            realityCgGroup.gameObject.SetActive(true);
            realityCgGroup.alpha = 0f;

            float t = 0f;
            while (t < realityFadeDuration)
            {
                t += Time.deltaTime;
                realityCgGroup.alpha = Mathf.Lerp(0f, 1f, t / realityFadeDuration);
                yield return null;
            }
            realityCgGroup.alpha = 1f;
        }

        // 关闭纸张画面
        if (interactionRoot != null)
            interactionRoot.SetActive(false);

        // 停留一段时间让玩家看清现实 CG
        if (realityHoldDuration > 0f)
            yield return new WaitForSeconds(realityHoldDuration);
    }

    private void OnDisable()
    {
        if (dripLoopAudioSource != null)
            dripLoopAudioSource.Stop();
    }
}