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

        Toggle toggle = ToolKitUtility.CreateToggle("Enable Action Button",ref toggleState, () =>
        {
            if (actionButton != null)
            {
                actionButton.SetEnabled(toggleState);
            }
        });
        root.Add(toggle);

        actionButton = ToolKitUtility.CreateButton("Action Button", () =>
        {
            Debug.Log("Action button clicked!");
        });
        
        root.Add(actionButton);

        Slider slider = ToolKitUtility.CreateSlider("Slider", 0f, 1f, sliderValue);
        root.Add(slider);

        Box inlineEditorBox = ToolKitUtility.CreateBox();
        ObjectField dataField = ToolKitUtility.CreateScriptableField("Select Data", typeof(ExampleData), data, dataEditor, rootVisualElement);
        inlineEditorBox.Add(dataField);
        
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

[CreateAssetMenu(fileName = "ExampleData", menuName = "Example/ExampleData")]
public class ExampleData : ScriptableObject
{
    public int randomInt;
    public string randomString;
    public string guidString;
}
