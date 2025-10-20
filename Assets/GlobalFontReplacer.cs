using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;

public class GlobalFontReplacer : EditorWindow
{
    private Font uiFont;
    private TMP_FontAsset tmpFont;

    [MenuItem("Tools/Global Font Replacer")]
    public static void ShowWindow()
    {
        GetWindow(typeof(GlobalFontReplacer), false, "Global Font Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Global Font Replacer", EditorStyles.boldLabel);
        GUILayout.Space(5);
        GUILayout.Label("Assign the new fonts to apply across your project.");

        uiFont = (Font)EditorGUILayout.ObjectField("UI Font (Legacy)", uiFont, typeof(Font), false);
        tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font", tmpFont, typeof(TMP_FontAsset), false);

        GUILayout.Space(10);

        if (GUILayout.Button("Replace Fonts in Current Scene"))
        {
            ReplaceFontsInScene();
        }

        if (GUILayout.Button("Replace Fonts in All Scenes + Prefabs"))
        {
            if (EditorUtility.DisplayDialog(
                "Confirm Global Font Replacement",
                "This will modify every prefab and scene in your project. Make sure to back up your project first!",
                "Proceed", "Cancel"))
            {
                ReplaceFontsEverywhere();
            }
        }
    }

    private void ReplaceFontsInScene()
    {
        int changedCount = 0;

        foreach (Text text in FindObjectsOfType<Text>(true))
        {
            if (uiFont && text.font != uiFont)
            {
                Undo.RecordObject(text, "Change UI Font");
                text.font = uiFont;
                EditorUtility.SetDirty(text);
                changedCount++;
            }
        }

        foreach (TMP_Text tmp in FindObjectsOfType<TMP_Text>(true))
        {
            if (tmpFont && tmp.font != tmpFont)
            {
                Undo.RecordObject(tmp, "Change TMP Font");
                tmp.font = tmpFont;
                EditorUtility.SetDirty(tmp);
                changedCount++;
            }
        }

        Debug.Log($"✅ Changed {changedCount} text components in current scene.");
    }

    private void ReplaceFontsEverywhere()
    {
        int totalChanges = 0;

        // Process all prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            bool changed = false;

            foreach (Text text in prefab.GetComponentsInChildren<Text>(true))
            {
                if (uiFont && text.font != uiFont)
                {
                    text.font = uiFont;
                    EditorUtility.SetDirty(prefab);
                    changed = true;
                    totalChanges++;
                }
            }

            foreach (TMP_Text tmp in prefab.GetComponentsInChildren<TMP_Text>(true))
            {
                if (tmpFont && tmp.font != tmpFont)
                {
                    tmp.font = tmpFont;
                    EditorUtility.SetDirty(prefab);
                    changed = true;
                    totalChanges++;
                }
            }

            if (changed)
                PrefabUtility.SavePrefabAsset(prefab);
        }

        // Process all scenes in Assets
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int before = totalChanges;
            ReplaceFontsInScene();
            int after = totalChanges;

            if (after > before)
                EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"🎉 Finished global font replacement! Total components changed: {totalChanges}");
    }
}