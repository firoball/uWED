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
            SimpleGenericNameProvider objectNames = new SimpleGenericNameProvider(names);
            m_mapObjectManipulator.SetProviders(objectNames, objectTextures);
            m_mapObjectManipulator.Open(mapObject);
        }
            
        private void OnEditVertex(Vertex vertex)
        {
            m_vertexManipulator.Open(vertex);
        }
        
        private void OnEditSegment(Segment segment, List<string> names)
        {
            SimpleGenericNameProvider segmentNames = new SimpleGenericNameProvider(names);
            m_segmentManipulator.SetProviders(segmentNames, segmentTextures);
            m_segmentManipulator.Open(segment);
        }

        private void OnEditRegion(Region region, List<string> names)
        {
            SimpleGenericNameProvider regionNames = new SimpleGenericNameProvider(names);
            m_regionManipulator.SetProviders(regionNames, regionTextures);
            m_regionManipulator.Open(region);
        }

        private void OnEditWay(Way way, List<string> names)
        {
            SimpleGenericNameProvider wayNames = new SimpleGenericNameProvider(names);
            m_wayManipulator.SetProviders(wayNames);
            m_wayManipulator.Open(way);
        }
    }
//}