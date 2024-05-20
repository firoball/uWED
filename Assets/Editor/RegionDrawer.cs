using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegionDrawer : BaseEditorDrawer
{
    private List<Segment> m_hoveredSegments;
    private List<Vector2> m_hoveredContour;
    private Tuple<Segment, bool> m_nearest;
    private bool m_inside;
    private Vector2 m_min;
    private Vector2 m_max;

    public RegionDrawer(MapData mapData) : base(mapData)
    {
        //m_enableNormals = true; //temp
    }

    public override void Initialize()
    {
        m_hoveredSegments = new List<Segment>();
        m_hoveredContour = new List<Vector2>();
        m_nearest = null;
        m_inside = false;
        m_min = new Vector2(float.MaxValue, float.MaxValue);
        m_max = new Vector2(float.MinValue, float.MinValue);
        base.Initialize();
    }

    protected override Color SetSegmentColor(int i)
    {
        Color color;
        /*if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && m_nearest.Item2) //TODO: temp - for testing only
            color = c_hoverColor;
        else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && !m_nearest.Item2) //TODO: temp - for testing only
            color = c_validColor;
        else*/ if (m_hoveredSegments.Contains(m_mapData.Segments[i]) && m_inside)
            color = c_validColor;
        else if (m_hoveredSegments.Contains(m_mapData.Segments[i]) && !m_inside)
            color = c_hoverColor;
        else if (m_mapData.Segments[i].Left == null && m_mapData.Segments[i].Right == null)
            color = c_invalidColor;
        else
            color = base.SetSegmentColor(i);

        return color;
    }

    protected override void ImmediateRepaint()
    {
        if (!enabledSelf)
            return;

        //TODO: can't this just be removed? If at all, test for 0 segments/vertices?
        if (m_mapData.Regions.Count == 0)
        {
            m_cursorInfo.Initialize();
            //return;
        }

        base.ImmediateRepaint();
        //MarkNextSegment();//TEMP
        /*if (m_hoveredContour.Count > 1)
        {
            EditorView ev = parent as EditorView;
            for (int i = 0; i < m_hoveredContour.Count; i++)
            {
                int j = (i + 1) % m_hoveredContour.Count;
                DrawLine(ev.WorldtoScreenSpace(m_hoveredContour[i]), ev.WorldtoScreenSpace(m_hoveredContour[j]), c_lineColor);
            }
        }*/
    }

    protected override void HoverTest()
    {
        Segment segment = FindNearestSegment(m_mapData.Segments);
        /*float minDist = float.MaxValue;
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Vector2 v1 = m_mapData.Segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = m_mapData.Segments[i].Vertex2.ScreenPosition;

            float sqrDist = Geom2D.PointToLineSqrDist(m_mousePos, v1, v2);
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                segment = m_mapData.Segments[i];
            }
        }*/

        EditorView ev = parent as EditorView;
        if (ev != null && segment != null)
        {
            Vector2 mouseWorldPos = ev.ScreenToWorldSpace(m_mousePos);
            bool left;
            if (Geom2D.IsCcw(segment.Vertex1.WorldPosition, segment.Vertex2.WorldPosition, mouseWorldPos))
            {
                m_cursorInfo.HoverRegion = segment.Left; //TODO: support creation of regions
                left = true;
            }
            else
            {
                m_cursorInfo.HoverRegion = segment.Right; //TODO: support creation of regions
                left = false;
            }

            //find complete contour independently from assigned regions
            Tuple<Segment, bool> nearest = new Tuple<Segment, bool>(segment, left);
            List<Vector2> hoveredContour = FindContour(nearest);
            m_inside = Geom2D.IsInside(hoveredContour, mouseWorldPos);
            m_nearest = nearest;
        }
        else
        {
            m_cursorInfo.HoverRegion = null;
        }
    }

    private Segment FindNearestSegment(List<Segment> segments)
    {
        Segment segment = null;
        float minDist = float.MaxValue;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 v1 = segments[i].Vertex1.ScreenPosition;
            Vector2 v2 = segments[i].Vertex2.ScreenPosition;

            float sqrDist = Geom2D.PointToLineSqrDist(m_mousePos, v1, v2);
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                segment = segments[i];
            }
        }
        return segment;
    }

    private List<Vector2> FindContour(Tuple<Segment, bool> nearest)
    {
        if (
            (m_nearest != null) && (nearest != null) &&
            ((m_nearest.Item1 != nearest.Item1) || (m_nearest.Item2 != nearest.Item2))
            )
        {
            List<Vector2> hoveredContour = new List<Vector2>();

            //init
            m_hoveredSegments.Clear();
            m_min = new Vector2(float.MaxValue, float.MaxValue);
            m_max = new Vector2(float.MinValue, float.MinValue);

            //iterate through connected segments
            Tuple<Segment, bool> first = nearest;
            Tuple<Segment, bool> current = first;
            do
            {
                current = ContourHelper.FindNextSegment(current);
                m_hoveredSegments.Add(current.Item1);
                Vector2 nextPos;
                if (current.Item2)
                    nextPos = current.Item1.Vertex1.WorldPosition;
                else
                    nextPos = current.Item1.Vertex2.WorldPosition;
                hoveredContour.Add(nextPos);

                //rect of contour
                m_min.x = Mathf.Min(nextPos.x, m_min.x);
                m_min.y = Mathf.Min(nextPos.y, m_min.y);
                m_max.x = Mathf.Max(nextPos.x, m_max.x);
                m_max.y = Mathf.Max(nextPos.y, m_max.y);

            } while (
                (current.Item1 != first.Item1 || current.Item2 != first.Item2) && 
                m_hoveredSegments.Count <= m_mapData.Segments.Count //avoid endless loop in case of some weird error
                );
            m_hoveredContour = hoveredContour; //Yikes (1)
        }

        return m_hoveredContour; //Yikes (2)
    }

    /*void MarkNextSegment() // temp
    {
        //TEMP - move to RegionMode
        if (m_nearest != null)
        {
            Segment s = m_nearest;
            Vertex v1 = s.Vertex1;
            Vertex v2 = s.Vertex2;
            if (m_left)
            {
                v1 = s.Vertex2;
                v2 = s.Vertex1;
            }

            if (v2.Connections.Count > 2)
            {
                float cw = float.MaxValue;
                float ccw = float.MinValue;
                Segment scw = null;
                Segment sccw = null;
                Vector2 lhs = (v2.WorldPosition - v1.WorldPosition).normalized;
                for (int i = 0; i < v2.Connections.Count; i++)
                {
                    if (v2.Connections[i] != s)
                    {
                        Vector2 rhs;
                        bool side;
                        if (v2.Connections[i].Vertex1 == v2)
                        {
                            rhs = (v2.Connections[i].Vertex2.WorldPosition - v2.WorldPosition).normalized;
                            side = Geom2D.IsCcw(v1.WorldPosition, v2.WorldPosition, v2.Connections[i].Vertex2.WorldPosition);
                        }
                        else
                        {
                            rhs = (v2.Connections[i].Vertex1.WorldPosition - v2.WorldPosition).normalized;
                            side = Geom2D.IsCcw(v1.WorldPosition, v2.WorldPosition, v2.Connections[i].Vertex1.WorldPosition);
                        }

                        float newdot = Vector2.Dot(lhs, rhs);
                        if (!side && newdot < cw)
                        {
                            cw = newdot;
                            scw = v2.Connections[i];
                        }
                        if (side && newdot > ccw)
                        {
                            ccw = newdot;
                            sccw = v2.Connections[i];
                        }
                    }
                }
                if (scw != null)
                {
                    DrawLine(scw.Vertex2.ScreenPosition, scw.Vertex1.ScreenPosition, c_validColor);
                }
                else
                {
                    DrawLine(sccw.Vertex2.ScreenPosition, sccw.Vertex1.ScreenPosition, c_validColor);
                }
            }
        }
    }*/
}