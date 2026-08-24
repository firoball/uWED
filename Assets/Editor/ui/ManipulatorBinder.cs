using System.Collections.Generic;
using Editor.UI.Manipulator;
using Editor.UI.View2D;
using UI.Controls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

//namespace Editor.ui
//{
    public class ManipulatorBinder
    {
        private readonly ITextureProvider segmentTextures = new SimpleTextureProvider();
        private readonly ITextureProvider regionTextures = new SimpleTextureProvider();
        private readonly ITextureProvider objectTextures = new SimpleTextureProvider();

        private readonly MapObjectManipulator m_mapObjectManipulator;
        private readonly VertexManipulator m_vertexManipulator;
        private readonly SegmentManipulator m_segmentManipulator;
        private readonly RegionManipulator m_regionManipulator;
        private readonly WayManipulator m_wayManipulator;

        private ComboBoxField textbox;
        public ManipulatorBinder(EditorView editorView, VisualTreeAsset uxml, VisualElement parent, IManipulatorSettings settings)
        {
            m_mapObjectManipulator = new MapObjectManipulator(uxml, settings);
            m_vertexManipulator = new VertexManipulator(uxml, settings);
            m_segmentManipulator = new SegmentManipulator(uxml, settings);
            m_regionManipulator = new RegionManipulator(uxml, settings);
            m_wayManipulator = new WayManipulator(uxml, settings);

            textbox = new ComboBoxField()
            {
                VisibleRowCount = 6, 
                AllowAdd = true,
                AllowDelete = false,//true,
                Ordering = ComboBoxSortMode.Ascending
            };
            textbox.style.marginTop = 50;
            string assetPath = "assets/editor/ui/comboboxfield/genericcomboboxfield.uss";
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (styleSheet != null)
                textbox.styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"StatisticsPanel: could not find stylesheet at '{assetPath}'. Panel will render unstyled.");
            
            parent.Add(textbox);

            parent.Add(m_mapObjectManipulator);
            parent.Add(m_vertexManipulator);
            parent.Add(m_segmentManipulator);
            parent.Add(m_regionManipulator);
            parent.Add(m_wayManipulator);

            editorView.Interface.OnEditObject += OnEditObject;
            editorView.Interface.OnEditVertex += OnEditVertex;
            editorView.Interface.OnEditSegment += OnEditSegment;
            editorView.Interface.OnEditRegion += OnEditRegion;
            editorView.Interface.OnEditWay += OnEditWay;
        }
        
        private void OnEditObject(MapObject mapObject, List<string> names)
        {
            SimpleNameProvider objectNames = new SimpleNameProvider(names);
            m_mapObjectManipulator.SetProviders(objectNames, objectTextures);
            m_mapObjectManipulator.Open(mapObject);

            textbox.Choices = names;
            textbox.value = mapObject.Name;
        }
            
        private void OnEditVertex(Vertex vertex)
        {
            m_vertexManipulator.Open(vertex);
        }
        
        private void OnEditSegment(Segment segment, List<string> names)
        {
            SimpleNameProvider segmentNames = new SimpleNameProvider(names);
            //m_segmentManipulator.SetProviders(segmentNames, segmentTextures);
            m_segmentManipulator.SetProviders(segmentNames, segmentTextures, segmentNames, segmentTextures);
            m_segmentManipulator.Open(segment);
        }

        private void OnEditRegion(Region region, List<string> names)
        {
            SimpleNameProvider regionNames = new SimpleNameProvider(names);
            m_regionManipulator.SetProviders(regionNames, regionTextures);
            m_regionManipulator.Open(region);
        }

        private void OnEditWay(Way way, List<string> names)
        {
            SimpleNameProvider wayNames = new SimpleNameProvider(names);
            m_wayManipulator.SetProviders(wayNames);
            m_wayManipulator.Open(way);
        }
    }
//}