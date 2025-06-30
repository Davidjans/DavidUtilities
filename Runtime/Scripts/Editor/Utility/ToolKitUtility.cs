using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public static class ToolKitUtility 
{
    public static Box CreateBox(string title = "InlineEditor",float marginTop = 10f, float paddingLeft = 5, float paddingRight = 5)
    {
        Box box = new Box();
        box.style.marginTop = marginTop;
        box.style.paddingLeft = paddingLeft;
        box.style.paddingRight = paddingRight;

        Label label = new Label(title);
        box.Add(label);
        return box;
    }

    public static Slider CreateSlider(string title = "Slider", float minValue = 0f, float maxValue = 1f, float defaultValue = 0.5f)
    {
        Slider slider = new Slider(title, minValue, maxValue)
        {
            value = defaultValue
        };
        slider.RegisterValueChangedCallback(evt =>
        {
            slider.value = evt.newValue;
        });
        return slider;
    }
    public static Toggle CreateLinkedToggle(string title ,Func<bool> getValue, Action<bool> setValue, Action toggleAction)
    {
        Toggle toggle = new Toggle(title)
        {
            value = getValue()
        };
        toggle.RegisterValueChangedCallback(evt =>
        {
            setValue(evt.newValue);
            toggleAction?.Invoke();
        });
        return toggle;
    }
    
    public static Toggle CreateToggle(string title ,ref bool toggleValueVariable, Action toggleAction)
    {
        Toggle toggle = new Toggle(title)
        {
            value = toggleValueVariable
        };
        toggle.RegisterValueChangedCallback(evt =>
        {
            toggle.value = evt.newValue;
            toggleAction?.Invoke();
        });
        return toggle;
    }
    
    public static Button CreateButton(string title, Action buttonAction)
    {
        Button button = new Button(() =>
        {
            buttonAction?.Invoke();
        })
        {
            text = title
        };
        return button;
    }
    
    public static ObjectField CreateObjectField(string title = "Select Object", System.Type objectType = null, Object defaultValue = null, Editor dataEditor = null, VisualElement rootVisualElement = null )
    {
        ObjectField objectField = new ObjectField(title)
        {
            objectType = objectType,
            value = defaultValue
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            Undo.RecordObject(evt.newValue, "Select New ExampleData");
            objectField.value = evt.newValue;
            
            if (dataEditor != null)
            {
                MonoBehaviour.DestroyImmediate(dataEditor);
            }
            
            if (objectField.value == null)
            {
                return;
            }
           
            dataEditor = Editor.CreateEditor(objectField.value);
            if (rootVisualElement != null) rootVisualElement.MarkDirtyRepaint();
        });
        
        objectField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue != null)
            {
                Undo.RecordObject(evt.newValue, "Select New Object");
            }
        });
        return objectField;
    }
    public static PopupField<string> CreatePopup(string title, List<string> options, string defaultValue, Action<string> onValueChanged)
    {
        PopupField<string> popup = new PopupField<string>(title, options, defaultValue);
        popup.RegisterValueChangedCallback(evt =>
        {
            onValueChanged?.Invoke(evt.newValue);
        });
        return popup;
    }

    public static string GetStyleTemplatePath(string templateName)
    {
        switch (templateName)
        {
            case "Dark":
                return "Assets/DavidUtilities/Runtime/Styles/Dark.uss";
            case "Light":
                return "Assets/DavidUtilities/Runtime/Styles/Light.uss";
            case "Cursed":
                return "Assets/DavidUtilities/Runtime/Styles/Cursed.uss";
            default:
                return "Assets/DavidUtilities/Runtime/Styles/Default.uss";
        }
    }
    
    public static VisualElement CreateInlineEditor(Editor editor)
    {
        var container = new VisualElement();
        container.Bind(editor.serializedObject);
        InspectorElement.FillDefaultInspector(container, editor.serializedObject, editor);
        return container;
    }
}
