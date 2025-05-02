using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ExampleToolWindow : EditorWindow
{
    private ExampleData data;
    private Editor dataEditor;

    private bool toggleState;
    private float sliderValue;
    private StyleSheet currentStyleSheet;

    [MenuItem("Window/Example Tool Window")]
    public static void ShowWindow()
    {
        ExampleToolWindow window = GetWindow<ExampleToolWindow>("Example Tool");
        window.Show();
    }

    private void OnEnable()
    {
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<ExampleData>();
            toggleState = true;
            sliderValue = 0.5f;
            data.randomInt = Random.Range(0, 100);
            data.randomString = "Hello World";
            data.guidString = System.Guid.NewGuid().ToString();
        }
        dataEditor = Editor.CreateEditor(data);
        BuildUI();
    }

    private void BuildUI()
    {
        VisualElement root = rootVisualElement;
        root.Clear();
        
        ObjectField styleSheetField = ToolKitUtility.CreateObjectField("Load StyleSheet", typeof(StyleSheet), currentStyleSheet, null, root);
        styleSheetField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue is StyleSheet newSheet)
            {
                if (currentStyleSheet != null)
                    root.styleSheets.Remove(currentStyleSheet);
                currentStyleSheet = newSheet;
                if (currentStyleSheet != null)
                    root.styleSheets.Add(currentStyleSheet);
            }
        });
        root.Add(styleSheetField);

        
        // popup for style sheeths
        var styleOptions = new System.Collections.Generic.List<string>() { "Default", "Dark", "Light" ,"Cursed"};
        PopupField<string> styleTemplatePopup = ToolKitUtility.CreatePopup(
            "Style Template", styleOptions, "Default", (newTemplate) =>
        {
            string path = ToolKitUtility.GetStyleTemplatePath(newTemplate);
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (sheet != null)
            {
                if (currentStyleSheet != null)
                    root.styleSheets.Remove(currentStyleSheet);
                currentStyleSheet = sheet;
                root.styleSheets.Add(currentStyleSheet);
            }
        });
        root.Add(styleTemplatePopup);

        Button actionButton = null;

        Toggle linkedtoggle = ToolKitUtility.CreateLinkedToggle(
            "Linked toggle example",
            () => data.linkedToggleExample,
            (newValue) => { data.linkedToggleExample = newValue; },
            () => Debug.Log("Linked toggle changed")
        );
        root.Add(linkedtoggle);

        Toggle toggle = ToolKitUtility.CreateToggle("Enable Action Button", ref toggleState, () =>
        {
            if (actionButton != null)
                actionButton.SetEnabled(toggleState);
        });
        root.Add(toggle);

        actionButton = ToolKitUtility.CreateButton("Action Button", () =>
        {
            Debug.Log("Action button clicked!");
        });
        root.Add(actionButton);

        Slider slider = ToolKitUtility.CreateSlider("Slider", 0f, 1f, sliderValue);
        slider.RegisterValueChangedCallback(evt =>
        {
            sliderValue = evt.newValue;
        });
        root.Add(slider);

        Box inlineEditorBox = ToolKitUtility.CreateBox("Inline Editor");
        ObjectField dataField = ToolKitUtility.CreateObjectField("Select Data", typeof(ExampleData), data, dataEditor, root);
        inlineEditorBox.Add(dataField);

        var inlineInspector = ToolKitUtility.CreateInlineEditor(dataEditor);
        inlineEditorBox.Add(inlineInspector);

        root.Add(inlineEditorBox);
    }
    
    private void OnDisable()
    {
        if (dataEditor != null)
            DestroyImmediate(dataEditor);
    }
}


/*
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class ExampleToolWindow : EditorWindow
{
    private ExampleData data;
    private Editor dataEditor;

    private bool toggleState;
    private float sliderValue;

    [MenuItem("Window/Example Tool Window")]
    public static void ShowWindow()
    {
        ExampleToolWindow window = GetWindow<ExampleToolWindow>("Example Tool");
        window.Show();
    }

    private void OnEnable()
    {
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<ExampleData>();
            toggleState= true;
            sliderValue = 0.5f;
            data.randomInt = Random.Range(0, 100);
            data.randomString = "Hello World";
            data.guidString = System.Guid.NewGuid().ToString();
        }
        dataEditor = Editor.CreateEditor(data);

        BuildUI();
    }

    private void BuildUI()
    {
        VisualElement root = rootVisualElement;
        root.Clear();
        
        Button actionButton = null;

        // linked toggle example. This can be used to for example manage your settings or change settings between different tools.
        Toggle linkedtoggle = ToolKitUtility.CreateLinkedToggle(
            "Linked toggle example", 
            () => data.linkedToggleExample, 
            (newValue) => { data.linkedToggleExample = newValue; }, 
            () => Debug.Log("Linked toggle changed")
        );
        root.Add(linkedtoggle);
        
        // normal toggle example
        Toggle toggle = ToolKitUtility.CreateToggle("Enable Action Button",ref toggleState, () =>
        {
            if (actionButton != null)
            {
                actionButton.SetEnabled(toggleState);
            }
        });
        root.Add(toggle);

        // button example
        actionButton = ToolKitUtility.CreateButton("Action Button", () =>
        {
            Debug.Log("Action button clicked!");
        });
        root.Add(actionButton);

        // slider example
        Slider slider = ToolKitUtility.CreateSlider("Slider", 0f, 1f, sliderValue);
        root.Add(slider);

        // how to put things in a box.
        Box inlineEditorBox = ToolKitUtility.CreateBox();
        
        // selection scriptableobjects or other unity object types.
        ObjectField dataField = ToolKitUtility.CreateScriptableField("Select Data", typeof(ExampleData), data, dataEditor, rootVisualElement);
        inlineEditorBox.Add(dataField);
        
        //creating a inline editor for the selected object.
        IMGUIContainer inlineEditorContainer = new IMGUIContainer(() =>
        {
            if (dataEditor != null)
            {
                dataEditor.OnInspectorGUI();
            }
        });
        inlineEditorBox.Add(inlineEditorContainer);

        root.Add(inlineEditorBox);
    }

    private void UpdateInlineEditorBox()
    {
        rootVisualElement.MarkDirtyRepaint();
    }

    private void OnDisable()
    {
        if (dataEditor != null)
        {
            DestroyImmediate(dataEditor);
        }
    }
}

*/

[CreateAssetMenu(fileName = "ExampleData", menuName = "Example/ExampleData")]
public class ExampleData : ScriptableObject
{
    public int randomInt;
    public string randomString;
    public string guidString;
    public bool linkedToggleExample;
    
}
/*#if UNITY_EDITOR
[CustomEditor(typeof(ExampleData))]
public class ExampleDataEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // root container for this inspector
        var root = new VisualElement();

        // debug header to verify we’re in UIElements land
        var header = new Label("UIElements Inspector");
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.marginBottom = 4;
        root.Add(header);

        // bind to serializedObject so PropertyFields auto‑update
        root.Bind(serializedObject);

        // add each field explicitly
        root.Add(new PropertyField(serializedObject.FindProperty("randomInt"),    "Random Int"));
        root.Add(new PropertyField(serializedObject.FindProperty("randomString"), "Random String"));
        root.Add(new PropertyField(serializedObject.FindProperty("guidString"),   "GUID"));
        root.Add(new PropertyField(serializedObject.FindProperty("linkedToggleExample"),
            "Linked Toggle"));

        // return the fully built UIElements inspector
        return root;
    }
}
#endif*/