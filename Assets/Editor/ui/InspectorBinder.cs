using System.Collections.Generic;
using System.Linq;
using Editor.UI.Inspector;
using Editor.UI.View2D;
using UnityEngine;

public class InspectorBinder
{
    private InfoPanel m_infoPanel;
    private MeshPreviewPanel m_meshPreviewPanel;
    private StatisticsPanel m_statistics;
    private EditorStatus.Mode m_mode;
    private EditorStatus.Construct m_construct;
    private Vector2 m_mousePos;

    public InspectorBinder(EditorView ev, InfoPanel infoPanel, MeshPreviewPanel meshPreviewPanel, StatisticsPanel statistics)
    {
        m_infoPanel = infoPanel;
        m_meshPreviewPanel = meshPreviewPanel;
        m_statistics = statistics;
        m_mode = EditorStatus.Mode.Objects;
        m_construct = EditorStatus.Construct.Idle;
        m_mousePos = Vector2.zero;

        ev.Interface.OnModeChanged += OnEditorModeChanged;
        ev.Interface.OnCursorInfoChanged += OnCursorInfoChanged;
        ev.Interface.OnRegionMeshChanged += OnRegionMeshChanged;
        ev.Interface.OnConstructionModeChanged += OnConstructionModeChanged;
        ev.Interface.OnMouseMoved += OnMouseMoved;
        
        m_infoPanel.ClearAll();

        //Update initial values
        ev.Interface.RefreshListeners();
    }

    private void OnEditorModeChanged(EditorStatus.Mode mode)
    {
        m_mode = mode;
        m_infoPanel.ClearAll();
        m_meshPreviewPanel.Clear();
    }

    private void OnCursorInfoChanged(CursorInfo ci)
    {
        InfoPanelStats ips = new InfoPanelStats()
        {
            TotalObjects = ci.Objects,
            TotalVertices = ci.Vertices,
            TotalSegments = ci.Segments,
            TotalRegions = ci.Regions,
            TotalWays = ci.Ways
        };
        m_statistics.SetStats(ips);

        //don't show panels during construction mode
        if (m_construct == EditorStatus.Construct.Constructing)
        {
            m_infoPanel.ClearAll();
            return;
        }
        
        switch (m_mode)
        {
            case EditorStatus.Mode.Objects:
                SetObject(ci.HoverObject, ci.SelectedObjects); 
                break;
            
            case  EditorStatus.Mode.Segments:
                if (ci.HoverVertex != null)
                    SetVertex(ci.HoverVertex, ci.SelectedVertices, ci.SelectedSegments);
                else
                    SetSegment(ci.HoverSegment, ci.SelectedVertices, ci.SelectedSegments);
                break;
            
            case EditorStatus.Mode.Regions:
                SetRegion(ci.HoverRegion, ci.SelectedRegions); 
                break;
            
            case EditorStatus.Mode.Ways:
                SetWay(ci.HoverVertex, null);
                break;
            
            default:
                m_infoPanel.ClearAll();
                break;
        }
    }

    private void OnRegionMeshChanged(Mesh mesh)
    {
        if (mesh != null)
            m_meshPreviewPanel.Set(mesh/*, m_material*/);
        else
            m_meshPreviewPanel.Clear();
    }
    
    private void OnConstructionModeChanged(EditorStatus.Construct mode)
    {
        m_construct = mode;
        m_statistics.SetExtra(m_mousePos.x, m_mousePos.y, m_construct.ToString());
    }

    private void OnMouseMoved(Vector2 mousePos)
    {
        m_mousePos = mousePos;
        m_statistics.SetExtra(m_mousePos.x, m_mousePos.y, m_construct.ToString());
    }
    
    private void SetObject(MapObject active, List<MapObject> selected)
    {
            m_infoPanel.SetHover(active, "tex");
            m_infoPanel.SetSelection(selected);
    }
    
    private void SetVertex(Vertex active, List<Vertex> selectedv, List<Segment> selecteds)
    {
        m_infoPanel.SetHover(active);
        m_infoPanel.SetSelection(selectedv, selecteds, "leftTex", "rightTex");
    }
    
    private void SetSegment(Segment active, List<Vertex> selectedv, List<Segment> selecteds)
    {
        m_infoPanel.SetHover(active, "leftTex", "rightTex");

        // selected segments always comes with its vertices. Special case single selected segment
        List<Vertex> verts;
        if (selecteds.Count == 1)
            verts = selectedv.Where(v => v != selecteds[0].Vertex1 && v != selecteds[0].Vertex2).ToList();
        else
            verts = selectedv;

        m_infoPanel.SetSelection(verts, selecteds, "leftTex", "rightTex");
    }
    
    private void SetRegion(Region active, List<Region> selected)
    {
        m_infoPanel.SetHover(active, 23, "floorTex", "ceilTex");
        m_infoPanel.SetSelection(selected, 23, "floorTex", "ceilTex");
    }
    
    private void SetWay(Vertex active, List<Way> selected)
    {
        m_infoPanel.SetHover(active);
    }
    
}
