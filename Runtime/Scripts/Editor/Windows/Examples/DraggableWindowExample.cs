using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace YourNamespace.Editor
{
   public class DraggableWindowExample : BaseDraggableWindow
    {
        readonly ExampleData  m_ExampleData;
        ListView          _exampledata2ListView;
        readonly SerializedObject m_SerializedData;
        public DraggableWindowExample(ExampleData exampleData, string title = "Example Settings", Vector2 size = default, Vector2 pos = default)
            : base(title, size == default ? new Vector2(360, 600) : size, pos == default ? new Vector2(20, 20) : pos)
        {
            objectToDirty = exampleData;
            m_ExampleData = exampleData;
            m_SerializedData = new SerializedObject(m_ExampleData);
            InitializeWindow();
        }

        protected override void BuildContent()
        {
            Content.Clear();
            
            m_SerializedData.Update();

            AddField(new IntegerField("example int"),             () => m_ExampleData.exampleInt,          v => m_ExampleData.exampleInt   = v);
            AddField(new Vector2Field("example vector2"),             () => m_ExampleData.exampleVector2,  v => m_ExampleData.exampleVector2          = v);
            AddField(new FloatField("example float"),           () => m_ExampleData.exampleFloat,               v => m_ExampleData.exampleFloat              = v);
            AddField(new Toggle("example bool"),          () => m_ExampleData.exampleBool,  v => m_ExampleData.exampleBool = v);

            AddObjectField<AudioClip>(
                "Music Track",
                () => m_ExampleData.exampleAudioCLip,
                v  => m_ExampleData.exampleAudioCLip = v
            );

            AddObjectField<GameObject>(
                "Collision Prefab",
                () => m_ExampleData.examplePrefab,
                v  => m_ExampleData.examplePrefab = v
            );
           

            var header = new VisualElement {
                style = {
                    flexDirection      = FlexDirection.Row,
                    justifyContent     = Justify.SpaceBetween,
                    alignItems         = Align.Center,
                    marginBottom       = 4
                }
            };
            header.Add(new Label("exampleData2 Settings") {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });
            header.Add(new Button(() => {
                m_ExampleData.exampleData2.Add(new ExampleData2());
                RefreshWeatherList();
                MarkDirty();
            }) { text = "+ Add" });
            Content.Add(header);

            _exampledata2ListView = new ListView(
                m_ExampleData.exampleData2,
                itemHeight: 42f,
                makeItem:  () => new ExampleData2Item(),
                bindItem:  (ve, i) => BindWeatherItem(ve, i)
            ) {
                reorderable                    = true,
                showBorder                     = true,
                showAlternatingRowBackgrounds  = AlternatingRowBackground.ContentOnly,
                style = { flexGrow = 1, minHeight = 100, maxHeight = 300 }
            };
            RefreshWeatherList();
            Content.Add(_exampledata2ListView);
        }

       

        void Apply()
        {
            m_SerializedData.ApplyModifiedProperties();
            MarkDirty(m_ExampleData);
        }
        
        

        

        void BindWeatherItem(VisualElement ve, int index)
        {
            var item = (ExampleData2Item)ve;
            item.Bind(
                m_ExampleData.exampleData2[index],
                onChanged: () => { MarkDirty(); },
                onRemove:  () => {
                    m_ExampleData.exampleData2.RemoveAt(index);
                    RefreshWeatherList();
                    MarkDirty();
                }
            );
        }

        void RefreshWeatherList()
        {
            _exampledata2ListView.itemsSource = m_ExampleData.exampleData2;
            _exampledata2ListView.Rebuild();
        }

        
    }

    public class ExampleData2Item : VisualElement
    {
        readonly TextField    _hourTypeField;
        readonly IntegerField _startHourField;
        readonly TextField    _weatherTypeField;
        readonly IntegerField _endHourField;
        readonly Button       _removeButton;

        ExampleData2 _data;
        Action         _onChanged;
        Action         _onRemove;

        public ExampleData2Item()
        {
            style.flexDirection  = FlexDirection.Row;
            style.height        = new StyleLength(StyleKeyword.Auto);
            style.paddingTop     = 4;
            style.paddingBottom  = 0;
            style.paddingLeft    = 4;
            style.paddingRight   = 4;
            style.alignItems     = Align.Stretch;

            var rows = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Column,
                    flexGrow      = 1
                }
            };

            var row1 = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems    = Align.Center,
                    marginBottom  = 2
                }
            };
            row1.Add(new Label("Hour:")   { style = { width = 50, marginRight = 4 } });
            _hourTypeField = new TextField { style = { flexGrow = 1, marginRight = 8 } };
            row1.Add(_hourTypeField);
            row1.Add(new Label("Start:")  { style = { width = 50, marginRight = 4 } });
            _startHourField = new IntegerField { style = { width = 60 } };
            row1.Add(_startHourField);
            rows.Add(row1);

            var row2 = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems    = Align.Center
                }
            };
            row2.Add(new Label("Weather:") { style = { width = 70, marginRight = 4 } });
            _weatherTypeField = new TextField { style = { flexGrow = 1, marginRight = 8 } };
            row2.Add(_weatherTypeField);
            row2.Add(new Label("End:")     { style = { width = 50, marginRight = 4 } });
            _endHourField = new IntegerField { style = { width = 60 } };
            row2.Add(_endHourField);
            rows.Add(row2);

            Add(rows);

            _removeButton = new Button(() => _onRemove?.Invoke()) { text = "🗑" };
            _removeButton.style.width     = 30;
            _removeButton.style.marginLeft= 8;
            _removeButton.style.alignSelf = Align.Stretch;
            Add(_removeButton);
        }

        public void Bind(ExampleData2 data, Action onChanged, Action onRemove)
        {
            _data      = data;
            _onChanged = onChanged;
            _onRemove  = onRemove;

            _hourTypeField.SetValueWithoutNotify(data.text);
            _startHourField.SetValueWithoutNotify(data.numberone);
            _weatherTypeField.SetValueWithoutNotify(data.texttwo);
            _endHourField.SetValueWithoutNotify(data.numberTwo);

            _hourTypeField.RegisterValueChangedCallback(evt => {
                data.text = evt.newValue;
                _onChanged();
            });
            _startHourField.RegisterValueChangedCallback(evt => {
                data.numberone = evt.newValue;
                _onChanged();
            });
            _weatherTypeField.RegisterValueChangedCallback(evt => {
                data.texttwo = evt.newValue;
                _onChanged();
            });
            _endHourField.RegisterValueChangedCallback(evt => {
                data.numberTwo = evt.newValue;
                _onChanged();
            });
        }
    }
}
