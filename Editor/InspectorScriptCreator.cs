using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Based on a script created by github.com/LotteMakesStuff.
/// </summary>
public static class InspectorScriptCreator
{
    [MenuItem("Assets/Create/Inspector Script", priority = 10)]
    public static void CreateClass()
    {
        foreach (var script in Selection.objects)
        {
            BuildEditorFile(script);
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/Create/Inspector Script", priority = 10, validate = true)]
    public static bool ValidateClass()
    {
        foreach (var script in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(script);

            if (script.GetType() != typeof(MonoScript))
                return false;
            if (!path.EndsWith(".cs"))
                return false;
            if (path.Contains("Editor"))
                return false;
        }

        return true;
    }

    public static void BuildEditorFile(Object obj)
    {
        var monoScript = obj as MonoScript;

        if (monoScript == null)
        {
            Debug.Log("ERROR: Couldn't generate an inspector script, selected script was not a MonoBehavior.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(obj);
        var filename = Path.GetFileNameWithoutExtension(assetPath);
        string script = "";
        string scriptNamespace = monoScript.GetClass().Namespace;

        if (scriptNamespace == null)
        {
            // No namespace, use the default template.
            script = string.Format(template, filename);
        }
        else
        {
            script = string.Format(namespaceTemplate, filename, scriptNamespace);
        }

        // Make sure that an Editor folder exists.       
        var editorFolder = Path.GetDirectoryName(assetPath) + "/Editor";

        if (Directory.Exists(editorFolder) == false)
        {
            Directory.CreateDirectory(editorFolder);
        }

        if (File.Exists(editorFolder + "/" + filename + "Inspector.cs") == true)
        {
            Debug.Log("ERROR: " +filename + "Inspector.cs already exists.");
            return;
        }

        File.WriteAllText(editorFolder + "/" + filename + "Inspector.cs", script);
    }

    #region Templates

    public static readonly string template = @"using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    [CustomEditor(typeof({0}))]
    //[CanEditMultipleObjects]
    public class {0}Inspector : Editor
    {{
        void OnEnable()
        {{
            // TODO: find properties we want to work with
            //serializedObject.FindProperty();
        }}
        public override void OnInspectorGUI()
        {{
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            // TODO: Draw UI here
            //EditorGUILayout.PropertyField();
            DrawDefaultInspector();
            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }}
    }}
    ";

    public static readonly string namespaceTemplate = @"using UnityEngine;
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    namespace {1}
    {{
        [CustomEditor(typeof({0}))]
        //[CanEditMultipleObjects]
        public class {0}Inspector : Editor
        {{
            void OnEnable()
            {{
                // TODO: find properties we want to work with
                //serializedObject.FindProperty();
            }}
            public override void OnInspectorGUI()
            {{
                // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
                serializedObject.Update();
                // TODO: Draw UI here
                //EditorGUILayout.PropertyField();
                DrawDefaultInspector();
                // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
                serializedObject.ApplyModifiedProperties();
            }}
        }}
    }}
    ";

    #endregion
}