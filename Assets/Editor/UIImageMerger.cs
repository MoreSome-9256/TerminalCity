using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIImageMerger : EditorWindow
{
    [MenuItem("GameObject/UI/合并子级静态 Image 为单张贴图", false, 0)]
    public static void MergeSelectedImages()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
        {
            EditorUtility.DisplayDialog("提示", "请先在 Hierarchy 中选中包含多个装饰 Image 的父物体！", "确定");
            return;
        }

        Image[] sourceImages = root.GetComponentsInChildren<Image>(false);
        if (sourceImages.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "该物体下没有处于激活状态的 Image 组件！", "确定");
            return;
        }

        // 1. 计算所有子级 Image 在 root 局部空间下的包围盒
        Matrix4x4 rootWorldToLocal = root.transform.worldToLocalMatrix;
        Bounds localBounds = new Bounds();
        bool hasBounds = false;

        foreach (var img in sourceImages)
        {
            if (img.sprite == null) continue;
            Vector3[] corners = new Vector3[4];
            img.rectTransform.GetWorldCorners(corners);
            foreach (var c in corners)
            {
                Vector3 localCorner = rootWorldToLocal.MultiplyPoint3x4(c);
                if (!hasBounds)
                {
                    localBounds = new Bounds(localCorner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(localCorner);
                }
            }
        }

        if (!hasBounds)
        {
            EditorUtility.DisplayDialog("提示", "子级 Image 均未设置 Sprite 贴图！", "确定");
            return;
        }

        int width = Mathf.CeilToInt(localBounds.size.x);
        int height = Mathf.CeilToInt(localBounds.size.y);

        if (width <= 0 || height <= 0 || width > 4096 || height > 4096)
        {
            EditorUtility.DisplayDialog("尺寸异常", $"计算出的包围盒尺寸为 {width}x{height}，请检查子物体范围。", "确定");
            return;
        }

        // 2. 创建目标 RenderTexture 并清空为全透明
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));

        // 设置正交投影矩阵，直接将 localBounds 区域映射到像素空间
        GL.PushMatrix();
        GL.LoadPixelMatrix(localBounds.min.x, localBounds.max.x, localBounds.min.y, localBounds.max.y);

        // 3. 使用 UI/Default Shader 逐个将子级 Sprite 绘制到 RenderTexture 上
        Shader uiShader = Shader.Find("UI/Default");
        if (uiShader == null) uiShader = Shader.Find("Unlit/Transparent");
        Material blitMat = new Material(uiShader);

        foreach (var img in sourceImages)
        {
            if (img.sprite == null || !img.enabled) continue;

            Sprite sp = img.sprite;
            Texture2D srcTex = sp.texture;
            if (srcTex == null) continue;

            // 计算该 Image 四个角在 root 局部空间下的坐标
            Vector3[] corners = new Vector3[4];
            img.rectTransform.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                corners[i] = rootWorldToLocal.MultiplyPoint3x4(corners[i]);
            }

            // 计算 Sprite 在图集/原图中的归一化 UV 区域
            Rect texRect = sp.textureRect;
            Rect uvRect = new Rect(
                texRect.x / srcTex.width,
                texRect.y / srcTex.height,
                texRect.width / srcTex.width,
                texRect.height / srcTex.height
            );

            // 应用 Image 自身的颜色与透明度
            Color finalColor = img.color;
            CanvasGroup[] groups = img.GetComponentsInParent<CanvasGroup>(false);
            foreach (var g in groups)
            {
                finalColor.a *= g.alpha;
            }

            // ✨ 关键修复：将当前 Sprite 的纹理绑定到材质的主纹理上
            blitMat.mainTexture = srcTex;
            blitMat.SetTexture("_MainTex", srcTex);
            blitMat.SetColor("_Color", finalColor);
            blitMat.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(finalColor);

            // GetWorldCorners 顺序：0=左下, 1=左上, 2=右上, 3=右下
            GL.TexCoord2(uvRect.xMin, uvRect.yMin);
            GL.Vertex3(corners[0].x, corners[0].y, 0);

            GL.TexCoord2(uvRect.xMin, uvRect.yMax);
            GL.Vertex3(corners[1].x, corners[1].y, 0);

            GL.TexCoord2(uvRect.xMax, uvRect.yMax);
            GL.Vertex3(corners[2].x, corners[2].y, 0);

            GL.TexCoord2(uvRect.xMax, uvRect.yMin);
            GL.Vertex3(corners[3].x, corners[3].y, 0);

            GL.End();
        }

        GL.PopMatrix();

        // 4. 从 RenderTexture 读取最终像素并保存为 PNG
        Texture2D resultTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        resultTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resultTex.Apply();

        RenderTexture.active = prevRT;
        RenderTexture.ReleaseTemporary(rt);
        DestroyImmediate(blitMat);

        string savePath = EditorUtility.SaveFilePanelInProject("保存合并后的贴图", root.name + "_Merged", "png", "选择保存路径");
        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllBytes(savePath, resultTex.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }

            Debug.Log($"[UIImageMerger] 合并成功！输出尺寸: {width}x{height}，路径: {savePath}");
        }

        DestroyImmediate(resultTex);
    }
}