using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class PopupSelection : EditorWindow
{
    private string description;
    private List<string> options;
    private Action<int> callback;
    private Vector2 scrollPos;

    public static void ShowWindow(string title, string description, List<string> options, Action<int> callback)
    {
        PopupSelection window = CreateInstance<PopupSelection>();
        window.titleContent = new GUIContent(title);
        window.description = description;
        window.options = options;
        window.callback = callback;
        window.minSize = new Vector2(250, 100);
        window.ShowUtility(); 
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < options.Count; i++)
        {
            if (GUILayout.Button(options[i]))
            {
                callback?.Invoke(i);
                Close();
            }
        }
        EditorGUILayout.EndScrollView();
    }
}