#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ScriptableObjectEditorExample : EditorWindow
{
    public StyleSheet styleSheet;
    
    // --- Data ---
    private List<MockDataObject> _dataObjects;
    private MockDataObject _selectedObject;
    
    // --- UI Elements ---
    private ListView _itemListView;
    private VisualElement _detailsPanel;
    private Label _detailsTitle;

    [MenuItem("Tools/Examples/ScriptableObject Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<ScriptableObjectEditorExample>();
        window.titleContent = new GUIContent("SO Editor Example");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        if (styleSheet != null) root.styleSheets.Add(styleSheet);

        // --- Create Structure ---
        var mainContainer = new VisualElement { name = "main-container" };
        var leftPane = new VisualElement { name = "left-pane" };
        var rightPane = new VisualElement { name = "right-pane" };
        mainContainer.Add(leftPane);
        mainContainer.Add(rightPane);
        root.Add(mainContainer);

        // --- Left Pane: List of Items ---
        _itemListView = new ListView { name = "item-list-view" };
        _itemListView.makeItem = () => new Label();
        _itemListView.bindItem = (element, i) => (element as Label).text = _dataObjects[i].itemName;
        _itemListView.onSelectionChange += OnObjectSelected;
        leftPane.Add(_itemListView);

        // --- Right Pane: Details of Selected Item ---
        _detailsPanel = rightPane;

        // --- Load Data ---
        LoadAllDataObjects();
        _itemListView.itemsSource = _dataObjects;
    }

    private void LoadAllDataObjects()
    {
        // Find all assets of type MockDataObject in the project
        _dataObjects = AssetDatabase.FindAssets($"t:{nameof(MockDataObject)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MockDataObject>)
            .OrderBy(item => item.name)
            .ToList();
    }

    private void OnObjectSelected(IEnumerable<object> selection)
    {
        _selectedObject = selection.FirstOrDefault() as MockDataObject;
        DrawDetailsPanel();
    }

    private void DrawDetailsPanel()
    {
        _detailsPanel.Clear();

        if (_selectedObject == null)
        {
            _detailsPanel.Add(new Label("Select an item from the list to edit it.") { name = "empty-state-label" });
            return;
        }

        _detailsTitle = new Label(_selectedObject.itemName) { name = "details-title" };
        _detailsPanel.Add(_detailsTitle);
        
        // This is the core of the editor.
        // We create a SerializedObject from our target. This handles all the
        // complex editor logic like saving, undo, and multi-object editing.
        var serializedObject = new SerializedObject(_selectedObject);
        
        // We iterate through the properties of the SerializedObject...
        var property = serializedObject.GetIterator();
        property.NextVisible(true); // Start with the first visible property

        // ...and create a PropertyField for each one.
        // PropertyField is a powerful UI Toolkit element that automatically creates
        // the correct editor UI (slider, color picker, enum dropdown, etc.) for a property.
        do
        {
            // We skip the default "m_Script" property
            if (property.name == "m_Script") continue;
            
            var propertyField = new PropertyField(property.Copy());
            
            // We bind the field to the SerializedObject. This is what enables
            // automatic saving, undo, and other editor features.
            propertyField.Bind(serializedObject);

            // Special handling to update the title if the name changes
            if (property.name == "itemName")
            {
                propertyField.RegisterValueChangeCallback(evt =>
                {
                    _detailsTitle.text = evt.changedProperty.stringValue;
                    _itemListView.Rebuild(); // Refresh list to show new name
                });
            }

            _detailsPanel.Add(propertyField);

        } while (property.NextVisible(false));
    }
}
#endif