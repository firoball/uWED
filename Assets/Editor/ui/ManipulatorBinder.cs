using System.Collections.Generic;
using Editor.UI.Manipulator;
using Editor.UI.View2D;
using UnityEngine.UIElements;

//namespace Editor.ui
//{
    public class ManipulatorBinder
    {
        ITextureProvider segmentTextures = new SimpleTextureProvider();
        ITextureProvider regionTextures = new SimpleTextureProvider();
        ITextureProvider objectTextures = new SimpleTextureProvider();

        private MapObjectManipulator m_mapObjectManipulator;
        private VertexManipulator m_vertexManipulator;
        private SegmentManipulator m_segmentManipulator;
        private RegionManipulator m_regionManipulator;
        private WayManipulator m_wayManipulator;

        public ManipulatorBinder(EditorView editorView, VisualTreeAsset uxml, VisualElement parent, IManipulatorSettings settings)
        {
            m_mapObjectManipulator = new MapObjectManipulator(uxml, settings);
            m_vertexManipulator = new VertexManipulator(uxml, settings);
            m_segmentManipulator = new SegmentManipulator(uxml, settings);
            m_regionManipulator = new RegionManipulator(uxml, settings);
            m_wayManipulator = new WayManipulator(uxml, settings);

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
            m_segmentManipulator.SetProviders(objectNames, objectTextures);
            m_mapObjectManipulator.Open(mapObject);
        }
            
        private void OnEditVertex(Vertex vertex)
        {
            m_vertexManipulator.Open(vertex);
        }
        
        private void OnEditSegment(Segment segment, List<string> names)
        {
            SimpleNameProvider segmentNames = new SimpleNameProvider(names);
            m_segmentManipulator.SetProviders(segmentNames, segmentTextures);
            //m_segmentManipulator.SetProviders(segmentNames, segmentTextures, segmentNames, segmentTextures);
            m_segmentManipulator.Open(segment);
        }

        private void OnEditRegion(Region region, List<string> names)
        {
            SimpleNameProvider regionNames = new SimpleNameProvider(names);
            SimpleNameProvider textureNames = new SimpleNameProvider(names);
            m_regionManipulator.SetProviders(regionNames, textureNames, regionTextures, textureNames, regionTextures);
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