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

    [Header("游戏提示 (UI Prompt)")]
    [Tooltip("挂载了 CanvasGroup 的提示 UI 节点")]
    public CanvasGroup promptGroup;
    public float promptFadeInTime = 1.0f;  // 提示淡入时间
    public float promptHoldTime = 2.0f;    // 提示停留时间
    public float promptFadeOutTime = 1.0f; // 提示淡出时间

    [Header("划痕设置")]
    public int requiredScratchCount = 5;     // 需要划满的次数
    public float minDragDistance = 120f;     // 单次有效划痕所需的最小像素距离
    public float minTimeBetweenCuts = 0.25f; // 防连击判定冷却

    [Header("转场现实 CG (前后景分离)")]
    public CanvasGroup realityBgGroup;       // 后景 CG（如背景环境）
    public CanvasGroup realityFgGroup;       // 前景 CG（如角色的手臂/身体）
    public float realityFadeDuration = 1.0f; // 切换到现实的淡入时间
    public float realityHoldDuration = 1.5f; // 现实 CG 停留时间
    [Header("CG 故障材质 (Shader Glitch)")]
    public Material glitchMaterial;           // 挂载了 UIAnalogGlitch 的材质球
    public float glitchFadeInTime = 1.5f;     // 故障效果逐渐达到最大的所需时间

    [Tooltip("最终故障强度目标值")]
    public float targetScanLineJitter = 0.02f;
    public float targetVerticalJump = 0.05f;
    public float targetHorizontalShake = 0.02f;
    public float targetColorDrift = 0.03f;

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

    [Header("现实 CG 血迹映射")]
    public RectTransform realityBloodContainer;
    public GameObject bloodPrefab;
    public Sprite[] bloodSprites;
    public Vector2 bloodScaleRange = new Vector2(0.8f, 1.2f);

    [Tooltip("血迹生成的微调偏移量（如希望血迹往下挪一点，可以调小Y值，如 -20）")]
    public Vector2 bloodSpawnOffset = new Vector2(0f, 0f);

    [Tooltip("转场时，原本白色的划痕会变成这个颜色")]
    public Color woundColor = new Color(0.6f, 0f, 0f, 1f); // 默认暗红色

    // 内部运行状态
    private Vector2 dragOffset;              // <--- 新增：保持抓取偏移，防止小刀瞬移吸附
    private Canvas parentCanvas;
    private Camera uiCamera;
    private int currentScratchCount = 0;
    private bool isInteracting = false;
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private float lastCutTime = 0f;
    private List<Vector2> cutLocalPositions = new List<Vector2>();

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

    [Header("故障转场效果 (Glitch Transition)")]
    [Tooltip("划完最后一刀后，纸张停留并产生故障效果的时间")]
    public float preTransitionWait = 2.0f;
    [Tooltip("用于实现发黑闪烁的全屏黑底 (建一个纯黑Image挂CanvasGroup)")]
    public CanvasGroup blackOverlay;
    [Tooltip("瞬间切入现实时的冲击/故障音效")]
    public AudioClip glitchSound;
    [Tooltip("画面震动的最大像素偏移幅度")]
    public float shakeIntensity = 15f;

    public override IEnumerator Execute()
    {
        ResetGlitchMaterial(); // 之前添加的材质重置

        currentScratchCount = 0;
        isInteracting = true;
        isDragging = false;
        hasCutInCurrentStroke = false;

        // 初始化显示
        if (interactionRoot != null)
            interactionRoot.SetActive(true);

        if (realityBgGroup != null) realityBgGroup.gameObject.SetActive(false);
        if (realityFgGroup != null) realityFgGroup.gameObject.SetActive(false);

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

        // 【新增】：开始异步显示提示文本，不阻塞玩家划刀
        if (promptGroup != null)
        {
            StartCoroutine(ShowPromptCoroutine());
        }

        // 等待玩家完成所有划痕
        while (currentScratchCount < requiredScratchCount)
        {
            HandleKnifeDragInput();
            yield return null;
        }

        isInteracting = false;

        yield return SwitchToRealityTransition();

        // ==========================================
        // 【新增】清理阶段：关闭现实CG和残留的划痕，交出屏幕控制权
        // ==========================================
        if (realityBgGroup != null) realityBgGroup.gameObject.SetActive(false);
        if (realityFgGroup != null) realityFgGroup.gameObject.SetActive(false);
        if (interactionRoot != null) interactionRoot.gameObject.SetActive(false);

        // 还原材质，防止下次运行或下个节点受影响
        ResetGlitchMaterial();
    }

    private IEnumerator ShowPromptCoroutine()
    {
        if (promptGroup == null) yield break;

        promptGroup.gameObject.SetActive(true);
        promptGroup.alpha = 0f;

        // 1. 淡入
        float t = 0f;
        while (t < promptFadeInTime)
        {
            t += Time.deltaTime;
            promptGroup.alpha = Mathf.Lerp(0f, 1f, t / promptFadeInTime);
            yield return null;
        }
        promptGroup.alpha = 1f;

        // 2. 停留
        if (promptHoldTime > 0f)
            yield return new WaitForSeconds(promptHoldTime);

        // 3. 淡出
        t = 0f;
        while (t < promptFadeOutTime)
        {
            t += Time.deltaTime;
            promptGroup.alpha = Mathf.Lerp(1f, 0f, t / promptFadeOutTime);
            yield return null;
        }
        promptGroup.alpha = 0f;
        promptGroup.gameObject.SetActive(false);
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
                                    // 把原来带 tipWorldPos 的那行改成这个：
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

    private void PerformCutAt(Vector2 cutLocalPos)
    {
        currentScratchCount++;
        float progress = Mathf.Clamp01((float)currentScratchCount / requiredScratchCount);

        // 准确记录划痕生成的相对坐标
        cutLocalPositions.Add(cutLocalPos);

        SpawnCenteredScratch(cutLocalPos);
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
        // ==========================================
        // 阶段 1：酝酿期 (仅发黑闪烁，取消画面抖动)
        // ==========================================

        // 锁死小刀操作
        if (knifeRect != null) knifeRect.gameObject.SetActive(false);

        float timer = 0f;

        // 记录原始位置用于后续的瞬间抖动还原
        Vector3 originalRootPos = interactionRoot != null ? interactionRoot.transform.localPosition : Vector3.zero;
        Vector3 originalBgPos = realityBgGroup != null ? realityBgGroup.transform.localPosition : Vector3.zero;
        Vector3 originalFgPos = realityFgGroup != null ? realityFgGroup.transform.localPosition : Vector3.zero;

        if (blackOverlay != null)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.alpha = 0f;
        }

        // 控制闪烁频率的变量
        float nextFlickerTime = 0f;
        float currentFlicker = 0f;

        while (timer < preTransitionWait)
        {
            timer += Time.deltaTime;
            float progress = timer / preTransitionWait;

            if (blackOverlay != null)
            {
                // 基础发黑进度（逐渐变黑至 90%）
                float baseDarkness = Mathf.Lerp(0f, 0.5f, progress);

                // 降低闪烁频率：每 0.08~0.15 秒才更新一次闪烁随机值，避免每帧狂闪
                if (Time.time >= nextFlickerTime)
                {
                    currentFlicker = Random.Range(-0.15f, 0.2f);
                    nextFlickerTime = Time.time + Random.Range(0.08f, 0.15f);
                }

                blackOverlay.alpha = Mathf.Clamp01(baseDarkness + currentFlicker);
            }
            yield return null;
        }

        // 酝酿结束，瞬间全黑
        if (blackOverlay != null) blackOverlay.alpha = 1f;

        // ==========================================
        // 阶段 2：故障爆发与瞬切 (Hard Cut)
        // ==========================================

        if (glitchSound != null && cutAudioSource != null)
        {
            cutAudioSource.pitch = 1f;
            cutAudioSource.PlayOneShot(glitchSound, 1f);
        }

        // 剥离划痕并染血
        if (scratchContainer != null && interactionRoot != null)
        {
            scratchContainer.SetParent(interactionRoot.transform, true);
        }
        if (paperRect != null) paperRect.gameObject.SetActive(false);

        if (scratchContainer != null)
        {
            Image[] scratches = scratchContainer.GetComponentsInChildren<Image>();
            foreach (Image img in scratches)
            {
                if (img.gameObject != scratchContainer.gameObject)
                {
                    img.color = woundColor;
                }
            }
        }

        SpawnMatchingBloodVisuals();

        // 瞬间开启前后景 CG（透明度直接设为 1）
        if (realityBgGroup != null)
        {
            realityBgGroup.gameObject.SetActive(true);
            realityBgGroup.alpha = 1f;
        }
        if (realityFgGroup != null)
        {
            realityFgGroup.gameObject.SetActive(true);
            realityFgGroup.alpha = 1f;
        }

        // ==========================================
        // 阶段 3：切换瞬间的剧烈抖动与画面亮起
        // ==========================================

        float flashTimer = 0f;
        float flashDuration = 0.25f; // 抖动和全黑消退维持的时间 (0.25秒)

        while (flashTimer < flashDuration)
        {
            flashTimer += Time.deltaTime;

            // 黑幕迅速消退
            if (blackOverlay != null)
            {
                blackOverlay.alpha = Mathf.Lerp(1f, 0f, flashTimer / flashDuration);
            }

            // 仅在这一瞬间产生剧烈抖动，且抖动幅度随这 0.25 秒快速衰减
            float currentShake = shakeIntensity * (1f - (flashTimer / flashDuration));
            Vector3 shakeOffset = new Vector3(
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake),
                0f
            );

            // 划痕、前景、后景一起震动，冲击力极强
            if (interactionRoot != null) interactionRoot.transform.localPosition = originalRootPos + shakeOffset;
            if (realityBgGroup != null) realityBgGroup.transform.localPosition = originalBgPos + shakeOffset;
            if (realityFgGroup != null) realityFgGroup.transform.localPosition = originalFgPos + shakeOffset;

            yield return null;
        }

        // 抖动结束，完全归位
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            blackOverlay.gameObject.SetActive(false);
        }
        if (interactionRoot != null) interactionRoot.transform.localPosition = originalRootPos;
        if (realityBgGroup != null) realityBgGroup.transform.localPosition = originalBgPos;
        if (realityFgGroup != null) realityFgGroup.transform.localPosition = originalFgPos;

        // 【新增】启动 Shader 逐渐故障的协程
        if (glitchMaterial != null)
        {
            StartCoroutine(AnimateGlitchEffect());
        }

        // 停留展示几秒钟
        if (realityHoldDuration > 0f)
            yield return new WaitForSeconds(realityHoldDuration);
    }

    private void SpawnMatchingBloodVisuals()
    {
        if (realityBloodContainer == null || bloodPrefab == null || bloodSprites == null || bloodSprites.Length == 0)
            return;

        foreach (Vector2 scratchLocalPos in cutLocalPositions)
        {
            // 核心修复：通过 TransformPoint/InverseTransformPoint 进行父子坐标系无损转换
            Vector3 worldPos = scratchContainer.TransformPoint(scratchLocalPos);
            Vector3 bloodLocalPos3D = realityBloodContainer.InverseTransformPoint(worldPos);

            // 叠加你在面板里手调的偏移量
            Vector2 finalPos = (Vector2)bloodLocalPos3D + bloodSpawnOffset;

            GameObject bloodObj = Instantiate(bloodPrefab, realityBloodContainer);
            RectTransform bloodRect = bloodObj.GetComponent<RectTransform>();
            Image bloodImg = bloodObj.GetComponent<Image>();

            Sprite chosenSprite = bloodSprites[Random.Range(0, bloodSprites.Length)];
            bloodImg.sprite = chosenSprite;
            bloodImg.SetNativeSize();

            bloodRect.localPosition = finalPos;

            float scale = Random.Range(bloodScaleRange.x, bloodScaleRange.y);
            float flipX = Random.value > 0.5f ? 1f : -1f;
            bloodRect.localScale = new Vector3(scale * flipX, scale, 1f);
        }
    }
    private IEnumerator AnimateGlitchEffect()
    {
        if (glitchMaterial == null) yield break;

        float timer = 0f;
        float shaderTime = 0f;

        // 无限循环，只要当前 PlotPoint 没被销毁或禁用，就一直驱动 Shader 动画
        while (true)
        {
            timer += Time.deltaTime;
            shaderTime += Time.deltaTime;

            // 计算 0 到 1 的渐变进度（钳制最大值为 1）
            float progress = Mathf.Clamp01(timer / glitchFadeInTime);
            // 使用 SmoothStep 让逐渐浮现的过程有“缓入缓出”的自然感
            float curve = Mathf.SmoothStep(0f, 1f, progress);

            // 动态修改 Shader 参数
            // _ScanLineJitter: x=强度, y=阈值[cite: 7]
            glitchMaterial.SetVector("_ScanLineJitter", new Vector2(targetScanLineJitter * curve, 0.1f));

            // _VerticalJump: x=强度, y=动画偏移时间[cite: 7]
            glitchMaterial.SetVector("_VerticalJump", new Vector2(targetVerticalJump * curve, shaderTime * 2f));

            // _HorizontalShake: 基础浮点数[cite: 7]
            glitchMaterial.SetFloat("_HorizontalShake", targetHorizontalShake * curve);

            // _ColorDrift: x=色偏强度, y=色偏动画时间[cite: 7]
            glitchMaterial.SetVector("_ColorDrift", new Vector2(targetColorDrift * curve, shaderTime * 3f));

            yield return null;
        }
    }

    private void OnDisable()
    {
        if (dripLoopAudioSource != null)
            dripLoopAudioSource.Stop();

        // 退出时彻底清零材质的故障参数
        ResetGlitchMaterial();
    }
    private void ResetGlitchMaterial()
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetVector("_ScanLineJitter", Vector2.zero);
            glitchMaterial.SetVector("_VerticalJump", Vector2.zero);
            glitchMaterial.SetFloat("_HorizontalShake", 0f);
            glitchMaterial.SetVector("_ColorDrift", Vector2.zero);
        }
    }
}