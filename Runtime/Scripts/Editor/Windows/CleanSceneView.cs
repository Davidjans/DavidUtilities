using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YourNamespace.Editor;

public class CleanSceneView : SceneView
{
    static StyleSheet s_StyleSheet;
    const string k_UssPath = "Assets/DavidUtilities/Runtime/Styles/CleanSceneView.uss";

    [MenuItem("Tools/Clean Scene View")]
    public static void OpenWindow()
    {
        GetWindow<CleanSceneView>().titleContent = new GUIContent("Clean Scene");
    }

    public override void OnEnable()
    {
        base.OnEnable();
        rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnFirstLayout);
    }

    void OnFirstLayout(GeometryChangedEvent evt)
    {
        rootVisualElement.UnregisterCallback<GeometryChangedEvent>(OnFirstLayout);

        SceneCleanup();
        LoadUSS();
        BuildTopToolbar();
    }

    public void SceneCleanup()
    {
        foreach (var overlay in this.overlayCanvas.overlays)
            overlay.displayed = false;

        this.drawGizmos = true;
        Tools.current = Tool.View;
        Selection.activeGameObject = null;
    }

    void LoadUSS()
    {
        if (s_StyleSheet == null)
            s_StyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(k_UssPath);

        if (s_StyleSheet != null)
            rootVisualElement.styleSheets.Add(s_StyleSheet);
    }

    void BuildTopToolbar()
    {
        var builtIn = rootVisualElement.Q<VisualElement>(className: "unity-scene-tool-bar");
        if (builtIn != null)
            builtIn.style.display = DisplayStyle.None;
        rootVisualElement.style.paddingTop = 0;
        rootVisualElement.style.marginTop  = 0;
        
        var bar = new VisualElement();
        bar.AddToClassList("top-bar");
        bar.style.position = Position.Absolute;
        
        var mapName = new Label("Example1");
        mapName.AddToClassList("example-1");
        bar.Add(mapName);

        var select = new Label("Example2");
        select.AddToClassList("example-2");
        bar.Add(select);

        var spacer = new VisualElement();
        spacer.AddToClassList("spacer");
        bar.Add(spacer);

        bar.Add(MakeButton("ButtonExample1",  () =>  Debug.Log("ButtonExample1 clicked")) );
        bar.Add(MakeButton("ButtonExample2",  () =>  Debug.Log("ButtonExample2 clicked")) );
        Button exampleDataButton = MakeButton("EXAMPLE DATA", null);
        exampleDataButton.clicked += () =>
        {
            ShowExampleData(exampleDataButton);
        };
        bar.Add(exampleDataButton);
        bar.Add(MakeButton("SAVE AS", null, isSave:true));

        rootVisualElement.Add(bar);
    }

    Button MakeButton(string text, Action click, bool isSave = false)
    {
        var btn = new Button(click) { text = text };
        btn.AddToClassList("btn");
        if (isSave)
            btn.AddToClassList("save-btn");
        return btn;
    }
    void ShowExampleData(VisualElement btn)
    {
        var root = rootVisualElement;

        Vector2 btnCenterBottom = btn.ChangeCoordinatesTo(
            root,
            new Vector2(btn.layout.width * 0.5f, btn.layout.height)
        );
        
        float panelWidth = 360; 
        float panelHeight = 600;
        Vector2 panelPos = new Vector2(
            btnCenterBottom.x - panelWidth * 0.5f,
            btnCenterBottom.y
        );

        var panel = new DraggableWindowExample(
            ExampleScene.instance.data,
            "Map Settings",
            size: new Vector2(panelWidth, panelHeight),
            pos: panelPos
        );
        root.Add(panel);
    }


}

