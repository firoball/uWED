using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.Platform;
using uWED.Runtime.UI.EventBus;
using uWED.Runtime.UI.Manipulator;
using Vertex = uWED.Runtime.Core.Map.Model.Vertex;

namespace uWED.Runtime.UI.Binder
{
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

        public ManipulatorBinder(VisualTreeAsset uxml, VisualElement parent, IManipulatorSettings settings)
        {
            // IMapObjectManipulatorProvider mirrors the Segment/Region providers below - lets an extension
            // supply its own MapObjectManipulator subclass without this class needing to know it exists.
            m_mapObjectManipulator = ServiceLocator.TryGet<IMapObjectManipulatorProvider>(out var mapObjectManipulatorProvider)
                ? mapObjectManipulatorProvider.Create(uxml, settings)
                : new MapObjectManipulator(uxml, settings);
            m_vertexManipulator = new VertexManipulator(uxml, settings);
            // ISegmentManipulatorProvider lets an extension (e.g. uWED.Acknex) supply its own
            // SegmentManipulator subclass without this class needing to know it exists - falls back to
            // the plain base class whenever no extension has registered one.
            m_segmentManipulator = ServiceLocator.TryGet<ISegmentManipulatorProvider>(out var segmentManipulatorProvider)
                ? segmentManipulatorProvider.Create(uxml, settings)
                : new SegmentManipulator(uxml, settings);
            // IRegionManipulatorProvider mirrors ISegmentManipulatorProvider above - lets an extension
            // supply its own RegionManipulator subclass without this class needing to know it exists.
            m_regionManipulator = ServiceLocator.TryGet<IRegionManipulatorProvider>(out var regionManipulatorProvider)
                ? regionManipulatorProvider.Create(uxml, settings)
                : new RegionManipulator(uxml, settings);
            m_wayManipulator = new WayManipulator(uxml, settings);

            parent.Add(m_mapObjectManipulator);
            parent.Add(m_vertexManipulator);
            parent.Add(m_segmentManipulator);
            parent.Add(m_regionManipulator);
            parent.Add(m_wayManipulator);

            EditorEventBus.Instance.EditObject.Subscribe(OnEditObject);
            EditorEventBus.Instance.EditVertex.Subscribe(OnEditVertex);
            EditorEventBus.Instance.EditSegment.Subscribe(OnEditSegment);
            EditorEventBus.Instance.EditRegion.Subscribe(OnEditRegion);
            EditorEventBus.Instance.EditWay.Subscribe(OnEditWay);
        }

        private void OnEditObject(MapObject mapObject, IReadOnlyDictionary<string, int> countByName)
        {
            SimpleGenericNameProvider objectNames = new SimpleGenericNameProvider(countByName.Keys.ToList());
            m_mapObjectManipulator.SetProviders(objectNames, objectTextures);
            m_mapObjectManipulator.SetCountByName(countByName);
            m_mapObjectManipulator.Open(mapObject);
        }

        private void OnEditVertex(Vertex vertex)
        {
            m_vertexManipulator.Open(vertex);
        }

        private void OnEditSegment(Segment segment, IReadOnlyDictionary<string, int> countByName)
        {
            SimpleGenericNameProvider segmentNames = new SimpleGenericNameProvider(countByName.Keys.ToList());
            m_segmentManipulator.SetProviders(segmentNames, segmentTextures);
            m_segmentManipulator.SetCountByName(countByName);
            m_segmentManipulator.Open(segment);
        }

        private void OnEditRegion(Region region, IReadOnlyDictionary<string, int> countByName)
        {
            SimpleGenericNameProvider regionNames = new SimpleGenericNameProvider(countByName.Keys.ToList());
            m_regionManipulator.SetProviders(regionNames, regionTextures);
            m_regionManipulator.SetCountByName(countByName);
            m_regionManipulator.Open(region);
        }

        private void OnEditWay(Way way, IReadOnlyDictionary<string, int> countByName)
        {
            SimpleGenericNameProvider wayNames = new SimpleGenericNameProvider(countByName.Keys.ToList());
            m_wayManipulator.SetProviders(wayNames);
            m_wayManipulator.SetCountByName(countByName);
            m_wayManipulator.Open(way);
        }
    }
}
