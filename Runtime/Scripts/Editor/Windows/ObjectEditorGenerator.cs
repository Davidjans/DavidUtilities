#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

public class ObjectEditorGenerator : EditorWindow
{
    private ObjectField scriptObjectField;
    private TextField savePathField;
    private Button generateButton;
    private VisualElement variableTogglesContainer;
    private Label feedbackLabel;

    private MonoScript selectedScript;
    private readonly Dictionary<string, Toggle> variableToggles = new Dictionary<string, Toggle>();

    [MenuItem("Tools/Object Editor Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<ObjectEditorGenerator>("Object Editor Generator");
        window.minSize = new Vector2(400, 300);
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        //root.style.padding = 10;
        root.Add(new Label("1. Select Target Script") { style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 5 } });
        scriptObjectField = new ObjectField { objectType = typeof(MonoScript), allowSceneObjects = false };
        scriptObjectField.RegisterValueChangedCallback(OnScriptSelected);
        root.Add(scriptObjectField);
        variableTogglesContainer = new VisualElement { style = { marginTop = 15, marginLeft = 10, borderLeftWidth = 2, borderLeftColor = Color.gray, paddingLeft = 10 } };
        root.Add(variableTogglesContainer);
        root.Add(new Label("2. Select Save Destination") { style = { fontSize = 14, unityFontStyleAndWeight = FontStyle.Bold, marginTop = 20, marginBottom = 5 } });
        var pathContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
        savePathField = new TextField { label = "Path", value = "Assets/Editor/Generated/", style = { flexGrow = 1 } };
        var browseButton = new Button(() =>
        {
            string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets/Editor", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                savePathField.value = path.EndsWith("/") ? path : path + "/";
            }
        }) { text = "Browse..." };
        pathContainer.Add(savePathField);
        pathContainer.Add(browseButton);
        root.Add(pathContainer);
        generateButton = new Button(GenerateScript) { text = "3. Generate Editor Script", style = { marginTop = 20, height = 30 } };
        generateButton.SetEnabled(false);
        root.Add(generateButton);
        feedbackLabel = new Label() { style = { marginTop = 10, unityFontStyleAndWeight = FontStyle.Italic } };
        root.Add(feedbackLabel);
    }

    private void OnScriptSelected(ChangeEvent<UnityEngine.Object> evt)
    {
        selectedScript = evt.newValue as MonoScript;
        variableTogglesContainer.Clear();
        variableToggles.Clear();
        generateButton.SetEnabled(false);
        feedbackLabel.text = "";
        if (selectedScript == null) return;
        Type scriptType = selectedScript.GetClass();
        if (scriptType == null || (!scriptType.IsSubclassOf(typeof(MonoBehaviour)) && !scriptType.IsSubclassOf(typeof(ScriptableObject))))
        {
            feedbackLabel.text = "Error: Selected script must derive from MonoBehaviour or ScriptableObject.";
            feedbackLabel.style.color = Color.red;
            return;
        }
        variableTogglesContainer.Add(new Label("Select public variables to include in the editor:") { style = { unityFontStyleAndWeight = FontStyle.Italic, marginBottom = 5 } });
        FieldInfo[] fields = scriptType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        if (fields.Length == 0)
        {
            variableTogglesContainer.Add(new Label("No public variables found in this script."));
            return;
        }
        foreach (var field in fields)
        {
            var toggle = new Toggle(field.Name) { value = true };
            variableTogglesContainer.Add(toggle);
            variableToggles[field.Name] = toggle;
        }
        generateButton.SetEnabled(true);
    }
    
    private void GenerateScript()
    {
        if (selectedScript == null)
        {
            EditorUtility.DisplayDialog("Error", "No script selected.", "OK");
            return;
        }

        string className = selectedScript.GetClass().Name;
        string newEditorClassName = $"{className}GeneratedEditor";
        string savePath = savePathField.value;

        if (string.IsNullOrEmpty(savePath) || !savePath.StartsWith("Assets/"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid save path. It must be a folder inside the project's Assets directory.", "OK");
            return;
        }

        // Create a list of the names of variables the user wants to include.
        var includedVariableNames = variableToggles
            .Where(pair => pair.Value.value)
            .Select(pair => pair.Key)
            .ToList();

        if (!includedVariableNames.Any())
        {
            EditorUtility.DisplayDialog("Warning", "No variables were selected. The generated script will not show any properties.", "OK");
        }

        Directory.CreateDirectory(savePath);
        string fullPath = Path.Combine(savePath, $"{newEditorClassName}.cs");

        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated by the ObjectEditorGenerator. Changes may be overwritten.");
        sb.AppendLine("#if UNITY_EDITOR");
        sb.AppendLine("using UnityEditor;");
        sb.AppendLine("using UnityEditor.UIElements;");
        sb.AppendLine("using UnityEngine.UIElements;");
        sb.AppendLine("using System.Collections.Generic;"); 
        sb.AppendLine();
        sb.AppendLine($"public class {newEditorClassName} : ObjectManagementEditor<{className}>");
        sb.AppendLine("{");
        sb.AppendLine($"    [MenuItem(\"Tools/Generated Editors/{className} Editor\")]");
        sb.AppendLine("    public static void ShowWindow()");
        sb.AppendLine("    {");
        sb.AppendLine($"        var window = GetWindow<{newEditorClassName}>();");
        sb.AppendLine($"        window.titleContent = new UnityEngine.GUIContent(\"{className} Editor\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    // A set containing the names of the properties to be displayed.");
        sb.AppendLine("    private readonly HashSet<string> _propertiesToShow = new HashSet<string>");
        sb.AppendLine("    {");
        foreach (var varName in includedVariableNames)
        {
            sb.AppendLine($"        \"{varName}\",");
        }
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    protected override void DrawDetailsPanelContent(SerializedObject serializedObject, VisualElement parent)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Iterate over all serialized properties of the object.");
        sb.AppendLine("        var property = serializedObject.GetIterator();");
        sb.AppendLine("        property.NextVisible(true); // Start with the first visible property.");
        sb.AppendLine();
        sb.AppendLine("        do");
        sb.AppendLine("        {");
        sb.AppendLine("            // If the property's name is in our set, create a field for it.");
        sb.AppendLine("            if (_propertiesToShow.Contains(property.name))");
        sb.AppendLine("            {");
        sb.AppendLine("                var propertyField = new PropertyField(property.Copy());");
        sb.AppendLine("                propertyField.Bind(serializedObject);");
        sb.AppendLine("                // The PropertyField will automatically bind to the property.");
        sb.AppendLine("                parent.Add(propertyField);");
        sb.AppendLine("            }");
        sb.AppendLine("        } while (property.NextVisible(false));");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine("#endif");

        File.WriteAllText(fullPath, sb.ToString());
        AssetDatabase.Refresh();

        feedbackLabel.text = $"Successfully generated editor script at: {fullPath}";
        feedbackLabel.style.color = new Color(0.2f, 0.7f, 0.2f);
        Debug.Log($"Generated editor script: {fullPath}", AssetDatabase.LoadAssetAtPath<MonoScript>(fullPath));
    }
}
#endif
