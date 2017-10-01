using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

[CustomEditor(typeof(MaskImage), true)]
[CanEditMultipleObjects]
public class MaskImageInspector : Editor
{
    private SerializedProperty m_UseRaycastMask;
    //private SerializedProperty m_RaycastAtten;

    private ImageEditor m_ImageEditor;

    [MenuItem("GameObject/UI/MaskImage")]
    static void CreateMaskImage()
    {
        Canvas[] canvas = FindObjectsOfType<Canvas>();
        Canvas root = null;
        if (canvas.Length <= 0)
        {
            if (EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas"))
            {
                canvas = FindObjectsOfType<Canvas>();
                if (canvas.Length > 0)
                {
                    root = canvas[0];
                }
            }
        }
        else
        {
            root = canvas[0];
        }
        if (root)
        {
            GameObject go = new GameObject("MaskImage");
            go.transform.SetParent(root.transform);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            go.AddComponent<MaskImage>();
            go.transform.localPosition = Vector3.zero;
        }
    }

    void OnEnable()
    {
        m_ImageEditor = (ImageEditor)ImageEditor.CreateEditor(target, typeof (ImageEditor));

        m_UseRaycastMask = serializedObject.FindProperty("useRaycastMask");
        //m_RaycastAtten = serializedObject.FindProperty("raycastAtten");
    }

    void OnDestroy()
    {
        if (m_ImageEditor)
            DestroyImmediate(m_ImageEditor);
        m_ImageEditor = null;
    }

    public override void OnInspectorGUI()
    {
        m_ImageEditor.OnInspectorGUI();
        EditorGUILayout.PropertyField(m_UseRaycastMask);
        serializedObject.ApplyModifiedProperties();
        //EditorGUILayout.PropertyField(m_RaycastAtten);
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        base.OnPreviewGUI(rect, background);
    }

}
