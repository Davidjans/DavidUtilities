#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public abstract class ObjectManagementEditor<T> : EditorWindow where T : Object
{
    protected VisualElement detailsPanel;
    protected ListView itemListView;

    protected List<T> allObjects;
    protected T selectedObject;

    protected abstract void DrawDetailsPanelContent(SerializedObject serializedObject, VisualElement parent);

    public void CreateGUI()
    {
        var mainContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
        var leftPane = new VisualElement { style = { width = 200, minWidth = 150, borderRightWidth = 1, borderRightColor = Color.gray } };
        detailsPanel = new VisualElement { style = { flexGrow = 1, paddingBottom = 10,paddingLeft = 10,paddingRight = 10,paddingTop = 10} };
        mainContainer.Add(leftPane);
        mainContainer.Add(detailsPanel);
        rootVisualElement.Add(mainContainer);

        itemListView = new ListView { style = { flexGrow = 1 } };
        itemListView.makeItem = () => new Label();
        itemListView.bindItem = (element, i) => (element as Label).text = allObjects[i].name;
        itemListView.onSelectionChange += OnObjectSelected;
        leftPane.Add(itemListView);

        LoadAllObjects();
        itemListView.itemsSource = allObjects;
        
        DrawEmptyDetailsPanel();
    }

    private void LoadAllObjects()
    {
        if (typeof(T).IsSubclassOf(typeof(ScriptableObject)))
        {
            allObjects = AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .OrderBy(item => item.name)
                .ToList();
        }
        else if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
        {
            allObjects = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(go => go.GetComponent<T>() != null)
                .Select(go => go.GetComponent<T>())
                .OrderBy(unit => unit.name)
                .ToList();
        }
        else
        {
            allObjects = new List<T>();
            Debug.LogError($"ObjectManagementEditor: Unsupported type '{typeof(T).Name}'. The target must be a ScriptableObject or MonoBehaviour.");
        }
    }

    private void OnObjectSelected(IEnumerable<object> selection)
    {
        selectedObject = selection.FirstOrDefault() as T;
        DrawDetailsPanel();
    }

    private void DrawDetailsPanel()
    {
        detailsPanel.Clear();

        if (selectedObject == null)
        {
            DrawEmptyDetailsPanel();
            return;
        }

        var serializedObject = new SerializedObject(selectedObject);

        var title = new Label(selectedObject.name) { name = "details-title", style = { fontSize = 18, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } };
        detailsPanel.Add(title);

        DrawDetailsPanelContent(serializedObject, detailsPanel);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEmptyDetailsPanel()
    {
        detailsPanel.Clear();
        var label = new Label($"Select a {typeof(T).Name} from the list to see its details.") { style = { unityFontStyleAndWeight = FontStyle.Italic, whiteSpace = WhiteSpace.Normal }};
        detailsPanel.Add(label);
    }
}
#endif
