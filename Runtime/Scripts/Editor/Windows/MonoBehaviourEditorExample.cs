// In /Assets/Editor/MB_EditorExample/MonoBehaviourEditorExample.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MonoBehaviourEditorExample : EditorWindow
{
    public StyleSheet styleSheet;
    
    private List<Unit> _unitPrefabs;
    private Unit _selectedUnit;
    
    private ListView _prefabListView;
    private VisualElement _detailsPanel;
    private Label _detailsTitle;

    [MenuItem("Tools/Examples/MonoBehaviour Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<MonoBehaviourEditorExample>();
        window.titleContent = new GUIContent("MB Editor Example");
    }

    public void CreateGUI()
    {
        var root = rootVisualElement;
        if(styleSheet != null) root.styleSheets.Add(styleSheet);
        
        var mainContainer = new VisualElement { name = "main-container" };
        var leftPane = new VisualElement { name = "left-pane" };
        var rightPane = new VisualElement { name = "right-pane" };
        mainContainer.Add(leftPane);
        mainContainer.Add(rightPane);
        root.Add(mainContainer);

        _prefabListView = new ListView { name = "prefab-list-view" };
        _prefabListView.makeItem = () => new Label();
        _prefabListView.bindItem = (element, i) => (element as Label).text = _unitPrefabs[i].unitName;
        _prefabListView.onSelectionChange += OnPrefabSelected;
        leftPane.Add(_prefabListView);

        _detailsPanel = rightPane;

        LoadAllPrefabs();
        _prefabListView.itemsSource = _unitPrefabs;
    }

    private void LoadAllPrefabs()
    {
        _unitPrefabs = AssetDatabase.FindAssets("t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
            .Where(go => go.GetComponent<Unit>() != null)
            .Select(go => go.GetComponent<Unit>())
            .OrderBy(unit => unit.name)
            .ToList();
    }

    private void OnPrefabSelected(IEnumerable<object> selection)
    {
        _selectedUnit = selection.FirstOrDefault() as Unit;
        DrawDetailsPanel();
    }

    private void DrawDetailsPanel()
    {
        _detailsPanel.Clear();

        if (_selectedUnit == null)
        {
            _detailsPanel.Add(new Label("Select a unit prefab from the list to edit it.") { name = "empty-state-label" });
            return;
        }

        _detailsTitle = new Label(_selectedUnit.unitName) { name = "details-title" };
        _detailsPanel.Add(_detailsTitle);
        
        var serializedObject = new SerializedObject(_selectedUnit);
        
        var property = serializedObject.GetIterator();
        property.NextVisible(true); 

        do
        {
            if (property.name == "m_Script") continue;
            
            var propertyField = new PropertyField(property.Copy());
            propertyField.Bind(serializedObject);

            if (property.name == "unitName")
            {
                propertyField.RegisterValueChangeCallback(evt =>
                {
                    _detailsTitle.text = evt.changedProperty.stringValue;
                    _prefabListView.Rebuild();
                });
            }

            _detailsPanel.Add(propertyField);

        } while (property.NextVisible(false));
    }
}
#endif
