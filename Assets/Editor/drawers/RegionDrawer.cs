using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Triangulator;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RegionDrawer : BaseEditorDrawer
{
    private class ContourRenderInfo
    {
        private List<int> m_triangles;
        private List<Vertex> m_vertices;

        public List<Vertex> Vertices { get =>  m_vertices; set =>  m_vertices = value; }
        public List<int> Triangles { get => m_triangles; set =>   m_triangles = value; }
    }
    
    private List<Contour> m_contours;
    private List<ContourRenderInfo> m_renderInfos;
    private Material m_editorMaterial;
    private Material m_polygonMaterial;
    private Mesh m_polygonMesh;
    
    private new class Colors : BaseEditorDrawer.Colors
    { 
        public static readonly Color LineDarkColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    }

    public RegionDrawer(MapData mapData) : base(mapData)
    {
        //m_enableNormals = true; //temp
    }

    public override void Initialize()
    {
        m_contours = new List<Contour>();
        m_renderInfos = new List<ContourRenderInfo>();
        FindContours();
        
        m_polygonMaterial = Resources.Load<Material>("testmaterial"); //TEMP
        PropertyInfo matProperty =
            typeof(HandleUtility).GetProperty("handleWireMaterial", BindingFlags.NonPublic | BindingFlags.Static);
        m_editorMaterial = (Material)matProperty.GetValue(null);
        m_polygonMesh = new Mesh();

        base.Initialize();
    }

    public override void SetSelectSingle()
    {
        if (m_cursorInfo.HoverRegion != null)
        {
            Region r = m_cursorInfo.HoverRegion;
            if (!m_cursorInfo.SelectedRegions.Contains(r))
                m_cursorInfo.SelectedRegions.Add(r);
            else
                m_cursorInfo.SelectedRegions.Remove(r);
        }
    }

    public override void Unselect()
    {
        m_cursorInfo.SelectedRegions.Clear();
    }


    protected override void SelectMultiple(Rect selection)
    {
        EditorView ev = parent as EditorView;
        if (ev != null)
        {
            foreach (Region r in m_mapData.Regions)
            {
                if (!m_cursorInfo.SelectedRegions.Contains(r) && 
                    selection.Contains(ev.WorldtoScreenSpace(r.Min)) && selection.Contains(ev.WorldtoScreenSpace(r.Max))
                    )
                    m_cursorInfo.SelectedRegions.Add(r);
            }
        }
    }


    protected override Color SetSegmentColor(int i)
    {
        Color color;
        /*if ((m_cursorInfo.HoverRegion != null) &&
            (m_mapData.Segments[i].Left == m_cursorInfo.HoverRegion || m_mapData.Segments[i].Right == m_cursorInfo.HoverRegion)
            )//existing region
            color = Colors.c_hoverColor;*/
        /*else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && m_nearest.Item2) //TODO: temp - for testing only
            color = Color.white;
        else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && !m_nearest.Item2) //TODO: temp - for testing only
            color = Color.blue;
        */
        /*else*/ if (m_cursorInfo.SelectedRegions.Contains(m_mapData.Segments[i].Left) || m_cursorInfo.SelectedRegions.Contains(m_mapData.Segments[i].Right))
            color = Colors.SelectColor;
        else if (m_cursorInfo.HoverContour != null && //segment is part of contour
                (m_cursorInfo.HoverContour.Is(m_mapData.Segments[i].CLeft) || m_cursorInfo.HoverContour.Is(m_mapData.Segments[i].CRight)) 
                )
            color = Colors.ValidColor; //TODO: proper coloring
        else if (m_mapData.Segments[i].Left != null && m_mapData.Segments[i].Right != null) //two regions assigned
            color = Colors.LineDarkColor;
        else if (m_mapData.Segments[i].Left != null || m_mapData.Segments[i].Right != null) //one region assigned
            color = Colors.LineColor;
        else //no region created yet
            color = Colors.InvalidColor;

        return color;
    }

    protected override void PreEditorRedraw(EditorView view)
    {
        if (m_cursorInfo.HoverContour != null && m_cursorInfo.HoverRegion != null)
        {
            m_polygonMaterial.SetPass(0);
            ContourRenderInfo cri = m_renderInfos[m_contours.IndexOf(m_cursorInfo.HoverContour)];
            m_polygonMesh.Clear();
            m_polygonMesh.vertices = cri.Vertices.Select(v => (Vector3)v.WorldPosition).ToArray();
            m_polygonMesh.uv = cri.Vertices.Select(v => v.WorldPosition).ToArray();
            m_polygonMesh.SetIndices(cri.Triangles, MeshTopology.Triangles, 0);
            Graphics.DrawMeshNow(m_polygonMesh, view.WorldToScreenMatrix());

            m_editorMaterial.SetPass(0);

            //List<Segment> segments = m_mapData.Segments
            //    .Where(x => x.CLeft.Is(m_cursorInfo.HoverContour) || x.CRight.Is(m_cursorInfo.HoverContour)).ToList();

            /*for (int i = 0; i < triangles.Count; i+= 3)
            {
                Vertex v1 = vertices[triangles[i]];
                Vertex v2 = vertices[triangles[i+1]];
                Vertex v3 = vertices[triangles[i+2]];
                if (segments.Count(s => (s.Vertex1 == v1 && s.Vertex2 == v2) || (s.Vertex1 == v2 && s.Vertex2 == v1)) == 0)
                    DrawLine(v1.ScreenPosition, v2.ScreenPosition, Colors.LineDarkColor);
                if (segments.Count(s => (s.Vertex1 == v3 && s.Vertex2 == v2) || (s.Vertex1 == v2 && s.Vertex2 == v3)) == 0)
                    DrawLine(v2.ScreenPosition, v3.ScreenPosition, Colors.LineDarkColor);
                if (segments.Count(s => (s.Vertex1 == v1 && s.Vertex2 == v3) || (s.Vertex1 == v3 && s.Vertex2 == v1)) == 0)
                    DrawLine(v3.ScreenPosition, v1.ScreenPosition, Colors.LineDarkColor);

            }*/

            List<(int, int)> diagonals = PolygonTriangulator.FindSplitDiagonals(m_cursorInfo.HoverContour, out List<Vertex> dvertices);
            foreach (var (v1, v2) in diagonals)
                DrawLine(dvertices[v1].ScreenPosition, dvertices[v2].ScreenPosition, Colors.HoverColor);

            /*Vector2 innerPoint = ev.WorldtoScreenSpace(m_cursorInfo.HoverContour.InnerPoint);
            DrawPoint(innerPoint, Colors.c_vertexColor, c_pointSize);
            */
        }
    }

    protected override void PostEditorRedraw(EditorView view)
    {
        
    }
    
    protected override void HoverTest()
    {
        EditorView ev = parent as EditorView;
        Tuple<Segment, bool> nearest = null;
        if (ev != null)
        {
            Vector2 mouseWorldPos = ev.ScreenToWorldSpace(m_mousePos);

            nearest = SegmentHelper.FindNearestSegment(m_mapData.Segments, mouseWorldPos);
            if (nearest != null)
            {
                Contour hoverContour;
                if (nearest.Item2) //left sided segment
                {
                    hoverContour = nearest.Item1.CLeft;
                    m_cursorInfo.HoverRegion = nearest.Item1.Left; //TODO: support creation of regions
                }
                else
                {
                    hoverContour = nearest.Item1.CRight;
                    m_cursorInfo.HoverRegion = nearest.Item1.Right; //TODO: support creation of regions
                }

                m_cursorInfo.HoverContour = hoverContour?.GetGroup();
            }
        }
        
        if (ev == null || nearest == null)
        {
            m_cursorInfo.HoverContour = null;
            m_cursorInfo.HoverRegion = null;
        }
    }

    private void FindContours()
    {
        List<Contour> inners = new List<Contour>();
        m_contours.Clear();

        //Step 1: Clear previous assignments
        for (int s = 0; s < m_mapData.Segments.Count; s++)
        {
            m_mapData.Segments[s].CLeft = null;
            m_mapData.Segments[s].CRight = null;
        }

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        //Step 2: Build all contours
        for (int s = 0; s < m_mapData.Segments.Count; s++)
        {
            if (m_mapData.Segments[s].CLeft == null)
                FindContour(m_mapData.Segments[s], true, inners);
            if (m_mapData.Segments[s].CRight == null)
                FindContour(m_mapData.Segments[s], false, inners);
        }
        stopWatch.Stop();
        Debug.Log($"contours built in {stopWatch.ElapsedMilliseconds}ms");
        stopWatch.Reset();
        stopWatch.Start();

        //Step 3: Attach inner contours to their outer contours
        for (int c = 0; c < m_contours.Count; c++) 
        {
            ContourHelper.FindInnerContour(m_contours[c], inners);
        }
        stopWatch.Stop();
        Debug.Log($"Inner contours attached in {stopWatch.ElapsedMilliseconds}ms");
        stopWatch.Reset();
        stopWatch.Start();
        for (int c = 0; c < m_contours.Count; c++)
        {
            ContourRenderInfo info = new ContourRenderInfo();
            try
            {
                info.Triangles = PolygonTriangulator.Triangulate(m_contours[c], out List<Vertex> vertices);
                info.Vertices = vertices;
            }
            catch 
            {
                if (m_contours[c].Repair())
                {
                    info.Triangles = PolygonTriangulator.Triangulate(m_contours[c], out List<Vertex> vertices);
                    info.Vertices = vertices;
                }
            }
            m_renderInfos.Add(info);
        }

        Debug.Log($"Polygons triangulated in {stopWatch.ElapsedMilliseconds}ms");
        stopWatch.Stop();

    }

    private void FindContour(Segment segment, bool left, List<Contour> inners)
    {
        Contour c = new Contour(segment, left, m_mapData.Segments.Count);
        if (c.IsInner)
            inners.Add(c);
        else
            m_contours.Add(c);
    }
    
}