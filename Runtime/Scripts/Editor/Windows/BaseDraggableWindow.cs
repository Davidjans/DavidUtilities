using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = System.Object;

public class BaseDraggableWindow : VisualElement
{
    public new class UxmlFactory : UxmlFactory<BaseDraggableWindow, UxmlTraits> { }
    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlStringAttributeDescription m_Title =
            new UxmlStringAttributeDescription { name = "title", defaultValue = "Window" };
        readonly UxmlFloatAttributeDescription m_Width =
            new UxmlFloatAttributeDescription { name = "width", defaultValue = 250 };
        readonly UxmlFloatAttributeDescription m_Height =
            new UxmlFloatAttributeDescription { name = "height", defaultValue = 150 };
        readonly UxmlFloatAttributeDescription m_PosX =
            new UxmlFloatAttributeDescription { name = "x", defaultValue = 50 };
        readonly UxmlFloatAttributeDescription m_PosY =
            new UxmlFloatAttributeDescription { name = "y", defaultValue = 50 };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var win = (BaseDraggableWindow)ve;
            win.Title      = m_Title.GetValueFromBag(bag, cc);
            win.WindowSize = new Vector2(
                m_Width.GetValueFromBag(bag, cc),
                m_Height.GetValueFromBag(bag, cc)
            );
            win.WindowPos  = new Vector2(
                m_PosX.GetValueFromBag(bag, cc),
                m_PosY.GetValueFromBag(bag, cc)
            );
            win.Build();
        }
    }

    public string Title { get; set; }
    public Vector2 WindowSize { get; set; }
    public Vector2 WindowPos  { get; set; }
    public UnityEngine.Object objectToDirty;
    public event Action OnClosed;
    public event Action OnOpened;

    VisualElement m_Header;
    bool m_Dragging;
    bool m_IsDragging;
    Vector2 m_PointerOffset;
    protected VisualElement Content { get; private set; }

    public BaseDraggableWindow()
        : this("Window", new Vector2(250, 150), new Vector2(50, 50))
    {
    }

    protected BaseDraggableWindow(string title, Vector2 size, Vector2 pos)
    {
        Title      = title;
        WindowSize = size;
        WindowPos  = pos;
    }

    protected StyleSheet GetDefaultDraggableStyle()
    {
        var sheet = Resources.Load<StyleSheet>("Styles/DefaultDraggableStyle");
        if (sheet == null)
        {
            Debug.LogWarning("DefaultDraggableStyle.uss not found in Resources folder.");
        }
        return sheet;
    }

    protected void InitializeWindow()
    {
        Build();
    }

    void Build()
    {
        var defaultStyle = GetDefaultDraggableStyle();
        if (defaultStyle != null)
        {
            styleSheets.Add(defaultStyle);
        }

        style.position      = Position.Absolute;
        style.left          = WindowPos.x;
        style.top           = WindowPos.y;
        style.width         = WindowSize.x;
        style.height        = new StyleLength(StyleKeyword.Auto);
        style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        style.borderTopWidth = style.borderBottomWidth =
            style.borderLeftWidth = style.borderRightWidth = 1;
        style.borderTopColor = style.borderBottomColor =
            style.borderLeftColor = style.borderRightColor = Color.black;
        style.maxHeight     = WindowSize.y;
        style.minHeight     = 0;
        AddToClassList("base-window");

        m_Header = new VisualElement { name = "header" };
        m_Header.AddToClassList("window-header");
        m_Header.style.flexDirection  = FlexDirection.Row;
        m_Header.style.alignItems     = Align.Center;
        m_Header.style.justifyContent = Justify.SpaceBetween;
        m_Header.style.height         = 24;
        Add(m_Header);

        var titleLabel = new Label(Title) { name = "title" };
        titleLabel.AddToClassList("window-title");
        m_Header.Add(titleLabel);

        var closeBtn = new Button(Close) { text = "✕", name = "close-btn" };
        closeBtn.AddToClassList("window-close");
        m_Header.Add(closeBtn);

        var scroll = new ScrollView(ScrollViewMode.Vertical)
        {
            name = "scrollview",
            verticalScrollerVisibility   = ScrollerVisibility.Auto,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        scroll.style.flexGrow = 1;
        Add(scroll);
        Content = scroll.contentContainer;

        m_Header.RegisterCallback<MouseDownEvent>(OnHeaderMouseDown);
        m_Header.RegisterCallback<MouseMoveEvent>(OnHeaderMouseMove);
        m_Header.RegisterCallback<MouseUpEvent>(OnHeaderMouseUp);
        m_Header.RegisterCallback<MouseLeaveEvent>(OnHeaderMouseUp);

        OnOpened?.Invoke();
        BuildContent();
    }

    protected virtual void BuildContent() { }

    void Close()
    {
        RemoveFromHierarchy();
        OnClosed?.Invoke();
    }

    void OnHeaderMouseDown(MouseDownEvent e)
    {
        if (e.button != 0) return;
        var left = style.left.value.value;
        var top  = style.top.value.value;
        m_PointerOffset = e.mousePosition - new Vector2(left, top);
        m_IsDragging = true;
        m_Header.CaptureMouse();
        e.StopPropagation();
    }

    void OnHeaderMouseMove(MouseMoveEvent e)
    {
        if (!m_IsDragging) return;
        var global = e.mousePosition;
        style.left = global.x - m_PointerOffset.x;
        style.top  = global.y - m_PointerOffset.y;
        e.StopPropagation();
    }

    void OnHeaderMouseUp(EventBase e)
    {
        if (!m_IsDragging) return;
        m_IsDragging = false;
        m_Header.ReleaseMouse();
        e.StopPropagation();
    }

    protected void AddField<TControl, TValue>(
        TControl field,
        Func<TValue> getter,
        Action<TValue> setter,
        UnityEngine.Object objectToDirty = null
    ) where TControl : BindableElement, INotifyValueChanged<TValue>
    {
        field.value = getter();
        field.RegisterValueChangedCallback(evt => {
            setter(evt.newValue);
            MarkDirty(objectToDirty);
        });
        Content.Add(field);
    }

    protected void AddObjectField<T>(
        string label,
        Func<T> getter,
        Action<T> setter
    ) where T : UnityEngine.Object
    {
        var field = new ObjectField(label)
        {
            objectType        = typeof(T),
            allowSceneObjects = false
        };
        field.value = (UnityEngine.Object)getter();
        field.RegisterValueChangedCallback((ChangeEvent<UnityEngine.Object> evt) =>
        {
            setter(evt.newValue as T);
            MarkDirty();
        });
        Content.Add(field);
    }

    protected void AddPropertyField(string propertyName, string label , SerializedObject serializedObject)
    {
        var prop = serializedObject.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogWarning($"MapSettingsWindow: could not find property '{propertyName}'");
            return;
        }
        var field = new PropertyField(prop, label);
        field.style.marginBottom = 4;
        field.Bind(serializedObject);
        Content.Add(field);
    }

    protected void MarkDirty(UnityEngine.Object obj = null)
    {
        if (obj == null)
        {
            if (objectToDirty == null)
            {
                return;
            }
            obj = objectToDirty;
        }
        EditorUtility.SetDirty(obj);
    }
}
