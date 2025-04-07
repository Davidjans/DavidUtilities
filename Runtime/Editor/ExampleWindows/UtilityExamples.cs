using DavidUtilities.CustomGUI;
using UnityEditor;
using UnityEngine;

public class UtilityExamples : EditorWindow
{
    private bool exampleBool;
    [MenuItem("Examples/UtilityExamples")]
    public static void ShowExample()
    {
        UtilityExamples wnd = GetWindow<UtilityExamples>();
        wnd.titleContent = new GUIContent("UtilityExamples");
    }

    public async void OnGUI()
    {
        GUILayout.Label("Button examples");
        DrawButtons();
        
        
    }

    void DrawButtons()
    {
        exampleBool = EditorGUILayout.Toggle("ExampleBool",exampleBool);
        if (DavidGUI.Button("Small Button", ButtonSize.Small))
        {
            Debug.Log("Clicked small button!");
        }

        if (DavidGUI.Button("Fanzy Button1", ButtonSize.Large, 0f,
                normalColor: new Color(0.4f, 0.8f, 1f),
                hoverColor: new Color(0.5f, 0.9f, 1f),
                textColor: Color.black))
        {
            Debug.Log("Clicked large button!");
        }

        if (DavidGUI.Button("Gigantic Button", ButtonSize.Gigantic))
        {
            Debug.Log("Gigantic clicked!");
        }

        if (DavidGUI.Button("Custom Sized Button", ButtonSize.Custom, 90f))
        {
            Debug.Log("Custom clicked!");
        }
    }
}
