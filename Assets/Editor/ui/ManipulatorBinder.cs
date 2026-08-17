using System.Collections.Generic;
using Editor.UI.Manipulator;
using Editor.UI.View2D;

//namespace Editor.ui
//{
    public class ManipulatorBinder
    {
        ITextureProvider segmentTextures = new SimpleTextureProvider();

        private VertexManipulator m_vertexManipulator;
        private SegmentManipulator m_segmentManipulator;

        public ManipulatorBinder(EditorView editorView, VertexManipulator vertexManipulator, SegmentManipulator segmentManipulator)
        {
            m_vertexManipulator = vertexManipulator;
            m_segmentManipulator = segmentManipulator;
            
            editorView.Interface.OnEditVertex += OnEditVertex;
            editorView.Interface.OnEditSegment += OnEditSegment;
        }
        
        private void OnEditVertex(Vertex vertex)
        {
            m_vertexManipulator.Open(vertex, vertex.Index);
        }
        
        private void OnEditSegment(Segment segment, List<string> names)
        {
            SimpleNameProvider segmentNames = new SimpleNameProvider(names);
            m_segmentManipulator.SetProviders(segmentNames, segmentTextures);
            m_segmentManipulator.Open(segment, segment.Index);
        }
    }
//}