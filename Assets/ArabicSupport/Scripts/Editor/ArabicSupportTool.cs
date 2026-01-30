#if UNITY_EDITOR
using ArabicSupport;
using UnityEditor;
using UnityEngine;

public class ArabicSupportTool : EditorWindow
{
    string rawText, fixedText;
    bool showTashkeel = true, useHinduNumbers = true;

    [MenuItem("Tools/Arabic Support Tool")]
    public static void ShowWindow() => GetWindow<ArabicSupportTool>("Arabic Support");

    void OnGUI() {
        fixedText = string.IsNullOrEmpty(rawText) ? "" : ArabicFixer.Fix(rawText, showTashkeel, useHinduNumbers);
        GUILayout.Label("Options:", EditorStyles.boldLabel);
        showTashkeel = EditorGUILayout.Toggle("Use Tashkeel", showTashkeel);
        useHinduNumbers = EditorGUILayout.Toggle("Use Hindu Numbers", useHinduNumbers);
        GUILayout.Label("Input (Not Fixed)", EditorStyles.boldLabel);
        rawText = EditorGUILayout.TextArea(rawText);
        GUILayout.Label("Output (Fixed)", EditorStyles.boldLabel);
        fixedText = EditorGUILayout.TextArea(fixedText);
        if (GUILayout.Button("Copy")) GUIUtility.systemCopyBuffer = fixedText;
    }
}
#endif