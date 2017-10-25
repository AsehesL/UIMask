using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
[DisallowMultipleComponent]
[AddComponentMenu("UI/MaskImage")]
public class MaskImage : Image
{
    public enum MaskType
    {
        Rect,
        Sprite,
    }

    public bool useRaycastMask = true;

    private float m_Atten;
    private Matrix4x4 m_MaskAreaMatrix;
    private Matrix4x4 m_OriginMaskAreaMatrix;

    private bool m_IsClear = true;

    private const string kAreaMaskShader = "UI/AreaMask";
    
    private RectTransform m_TargetRectTransform;
    private RectTransform m_FollowRectTransform;
    private bool m_FollowMask;
    private Matrix4x4 m_OriginTargetTransformMatrix;
    private Matrix4x4 m_OriginFollowTransformMatrix;

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
#if DEBUG
        ResetMaterial(m_TargetRectTransform);
#endif
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (base.IsRaycastLocationValid(screenPoint, eventCamera))
        {
            if (!useRaycastMask)
            {
                return true;
            }
            else
            {
                if (m_IsClear)
                    return true;
                Vector3 worldPos = eventCamera == null ? (Vector3)screenPoint : eventCamera.ScreenToWorldPoint(screenPoint);
                worldPos = canvas.transform.worldToLocalMatrix.MultiplyPoint(worldPos);
                worldPos = m_MaskAreaMatrix.MultiplyPoint(worldPos);
                if (worldPos.x >= 0 && worldPos.x <= 1 && worldPos.y >= 0 && worldPos.y <= 1)
                {
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        type = Type.Simple;
        if (material == null)
            material = new Material(Shader.Find(kAreaMaskShader));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (material)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
                DestroyImmediate(material);
            else
                Destroy(material);
        }
        m_FollowRectTransform = null;
        m_TargetRectTransform = null;
    }

    void Update()
    {
        if (m_FollowMask)
        {
            if (m_FollowRectTransform && m_OriginFollowTransformMatrix != m_FollowRectTransform.localToWorldMatrix)
            {
                m_OriginFollowTransformMatrix = m_FollowRectTransform.localToWorldMatrix;
                CopyRectTransform(m_FollowRectTransform);
            }
        }
        if (m_TargetRectTransform && m_OriginTargetTransformMatrix != m_TargetRectTransform.localToWorldMatrix)
        {
            m_OriginTargetTransformMatrix = m_TargetRectTransform.localToWorldMatrix;
            ResetMaterial(m_TargetRectTransform);
        }
    }

    /// <summary>
    /// 显示蒙版
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetTransform">蒙版区域RectTransform</param>
    public void ShowMask(Color color, float atten, RectTransform targetTransform)
    {
        ShowMask(color, atten, MaskType.Rect, targetTransform, null);
    }

    /// <summary>
    /// 显示蒙版
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetImage">蒙版区域Image</param>
    public void ShowMask(Color color, float atten, Image targetImage)
    {
        ShowMask(color, atten, MaskType.Sprite, targetImage.rectTransform, targetImage.sprite);
    }

    private void ShowMask(Color color, float atten, MaskType maskType, RectTransform targetTransform, Sprite sprite)
    {
        SetMask(color, atten, maskType, targetTransform, sprite, false);
    }
    /// <summary>
    /// 显示遮罩世界空间物体的蒙版
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetTransform">目标物体</param>
    /// <param name="size">大小</param>
    /// <param name="angle">角度</param>
    public void ShowMaskToWorldTarget(Color color, float atten, Transform targetTransform, Vector2 size, float angle)
    {
        SetMaskFromWorld(color, atten, targetTransform, size, angle);
    }
    /// <summary>
    /// 显示遮罩世界空间物体的蒙版
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetTransform">目标物体</param>
    /// <param name="size">大小</param>
    public void ShowMaskToWorldTarget(Color color, float atten, Transform targetTransform, Vector2 size)
    {
        SetMaskFromWorld(color, atten, targetTransform, size, 0);
    }

    /// <summary>
    /// 设置跟随蒙版-跟随蒙版会随着蒙版目标的移动和旋转自动调整蒙版位置
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetTransform">蒙版区域RectTransform</param>
    public void SetFollowMask(Color color, float atten, RectTransform targetTransform)
    {
        SetFollowMask(color, atten, MaskType.Rect, targetTransform, null);
    }

    /// <summary>
    /// 设置跟随蒙版-跟随蒙版会随着蒙版目标的移动和旋转自动调整蒙版位置
    /// </summary>
    /// <param name="color">蒙版颜色</param>
    /// <param name="atten">衰减(1-0  1表示不衰减  0表示完全衰减)</param>
    /// <param name="targetImage">蒙版区域Image</param>
    public void SetFollowMask(Color color, float atten, Image targetImage)
    {
        SetFollowMask(color, atten, MaskType.Sprite, targetImage.rectTransform, targetImage.sprite);
    }

    private void SetFollowMask(Color color, float atten, MaskType maskType, RectTransform targetTransform, Sprite sprite)
    {
        SetMask(color, atten, maskType, targetTransform, sprite, true);
    }

    /// <summary>
    /// 清除蒙版
    /// </summary>
    public void ClearMask()
    {
        if (!m_IsClear)
            material.SetVector("internalClipAtten", new Vector2(m_Atten, 0));
        m_IsClear = true;
    }

    private void SetMaskFromWorld(Color color, float atten, Transform targetTransform, Vector2 size, float angle)
    {
        enabled = true;
        this.color = color;
        //this.sprite = null;
        material.SetTexture("_MaskTex", null);

        SetTargetRectTransformFromWorld(targetTransform, size, angle);
        ResetMaterial(m_TargetRectTransform);
        m_FollowMask = false;
        atten = Mathf.Clamp01(atten) * 0.5f;
        if (m_Atten != atten || m_IsClear)
        {
            material.SetVector("internalClipAtten", new Vector2(atten, 1));
            m_Atten = atten;
        }
        m_IsClear = false;
    }

    private void SetMask(Color color, float atten, MaskType maskType, RectTransform targetTransform, Sprite sprite,
        bool setFollow)
    {
        enabled = true;
        this.color = color;
        if (maskType == MaskType.Sprite)
        {
            material.SetTexture("_MaskTex", sprite.texture);

            float w = sprite.rect.width/sprite.texture.width;
            float h = sprite.rect.height/sprite.texture.height;
            float x = (sprite.textureRect.x - sprite.textureRectOffset.x)/sprite.texture.width;
            float y = (sprite.textureRect.y - sprite.textureRectOffset.y)/sprite.texture.height;

            material.SetVector("_Offset", new Vector4(w, h, x, y));
            //this.sprite = sprite;
        }
        else
            material.SetTexture("_MaskTex", null);
        //this.sprite = null;
        ResetMaterial(targetTransform);

        CopyRectTransform(targetTransform);
        //m_TargetRectTransform = targetTransform;
        m_FollowMask = setFollow;
        if (m_FollowMask)
            m_FollowRectTransform = targetTransform;

        atten = Mathf.Clamp01(atten) * 0.5f;
        if (m_Atten != atten || m_IsClear)
        {
            material.SetVector("internalClipAtten", new Vector2(atten, 1));
            m_Atten = atten;
        }
        m_IsClear = false;
    }
    
    private void ResetMaterial(RectTransform targetTransform)
    {
        if (targetTransform == null)
            return;
        m_MaskAreaMatrix = default(Matrix4x4);
        m_MaskAreaMatrix.m00 = 1 / targetTransform.rect.width;
        m_MaskAreaMatrix.m03 = -targetTransform.rect.x / targetTransform.rect.width;
        m_MaskAreaMatrix.m11 = 1 / targetTransform.rect.height;
        m_MaskAreaMatrix.m13 = -targetTransform.rect.y / targetTransform.rect.height;
        m_MaskAreaMatrix.m33 = 1;
        
        Matrix4x4 ltw = default(Matrix4x4);
        ltw.m00 = targetTransform.right.x;
        ltw.m01 = targetTransform.up.x;
        ltw.m03 = targetTransform.position.x;

        ltw.m10 = targetTransform.right.y;
        ltw.m11 = targetTransform.up.y;
        ltw.m13 = targetTransform.position.y;
        
        ltw.m22 = 1;
        ltw.m23 = targetTransform.position.z;
        
        ltw.m33 = 1;

        m_MaskAreaMatrix = m_MaskAreaMatrix * ltw.inverse * canvas.transform.localToWorldMatrix;
        if (m_OriginMaskAreaMatrix != m_MaskAreaMatrix)
        {
            material.SetMatrix("internalWorldToMaskMatrix", m_MaskAreaMatrix);
            m_OriginMaskAreaMatrix = m_MaskAreaMatrix;
        }
    }

    private void CopyRectTransform(RectTransform targetTransform)
    {
        if (targetTransform == null)
            return;
        if (m_TargetRectTransform == null)
        {
            m_TargetRectTransform = new GameObject("[Mask]").AddComponent<RectTransform>();
            m_TargetRectTransform.SetParent(transform);
        }
        m_TargetRectTransform.anchorMax = targetTransform.anchorMax;
        m_TargetRectTransform.anchorMin = targetTransform.anchorMin;
        m_TargetRectTransform.pivot = targetTransform.pivot;
        m_TargetRectTransform.rotation = targetTransform.rotation;
        m_TargetRectTransform.localScale = targetTransform.localScale;
        m_TargetRectTransform.sizeDelta = targetTransform.sizeDelta;
        m_TargetRectTransform.position = targetTransform.position;
    }

    private void SetTargetRectTransformFromWorld(Transform targetTransform, Vector2 size, float angle)
    {
        if (m_TargetRectTransform == null)
        {
            m_TargetRectTransform = new GameObject("[Mask]").AddComponent<RectTransform>();
            m_TargetRectTransform.SetParent(transform);
        }
        m_TargetRectTransform.anchorMax = m_TargetRectTransform.anchorMin = m_TargetRectTransform.pivot = new Vector2(0.5f, 0.5f);
        m_TargetRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        m_TargetRectTransform.localScale = Vector3.one;
        m_TargetRectTransform.sizeDelta = size;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);

        //screenPos.x -= Screen.width;
        //screenPos.y -= Screen.height;
        screenPos.x = screenPos.x-((float)Screen.width)/2;
        screenPos.y = screenPos.y -((float)Screen.height)/2;
        m_TargetRectTransform.anchoredPosition3D = screenPos;
    }
}
