using System;
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
    
    public static Toggle CreateToggle(string title ,ref bool toggleValueVariable, Action toggleAction)
    {
        Toggle toggle = new Toggle("Enable Action Button")
        {
            value = toggleValueVariable
        };
        toggle.RegisterValueChangedCallback(evt =>
        {
            toggleValueVariable = evt.newValue;
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
    
    /// <summary>
    /// Creates a field to drop a scriptable object in. To either be edited in the inspector or just to grab the data from.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="objectType"></param>
    /// <param name="defaultValue"></param>
    /// <param name="dataEditor">Technically not necesarry but will cause issues with redo and undo if not present</param>
    /// <param name="rootVisualElement"> Technically not necesarry but will cause issues with redo and undo if not present</param>
    /// <returns></returns>
    public static ObjectField CreateScriptableField(string title = "Select Object", System.Type objectType = null, Object defaultValue = null, Editor dataEditor = null, VisualElement rootVisualElement = null )
    {
        ObjectField objectField = new ObjectField(title)
        {
            objectType = objectType,
            value = defaultValue
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue is ExampleData newData)
            {
                Undo.RecordObject(newData, "Select New ExampleData");
                objectField.value = newData;

                if (objectField.value == null)
                {
                    return;
                }
                if (dataEditor != null)
                {
                    MonoBehaviour.DestroyImmediate(dataEditor);
                }
                dataEditor = Editor.CreateEditor(objectField.value);
                if (rootVisualElement != null) rootVisualElement.MarkDirtyRepaint();
            }
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
    
}
