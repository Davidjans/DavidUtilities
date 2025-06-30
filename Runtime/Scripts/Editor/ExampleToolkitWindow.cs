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

    [MenuItem("Tools/Example Tool Window")]
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
            data.exampleInt = Random.Range(0, 100);
            data.exampleString = "Hello World";
            //data.guidString = System.Guid.NewGuid().ToString();
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
            () => data.exampleBool,
            (newValue) => { data.exampleBool = newValue; },
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

