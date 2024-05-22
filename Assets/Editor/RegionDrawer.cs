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
        base.Initialize();
    }

    protected override Color SetSegmentColor(int i)
    {
        Color color;
        /*if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && m_nearest.Item2) //TODO: temp - for testing only
            color = Color.white;
        else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && !m_nearest.Item2) //TODO: temp - for testing only
            color = Color.blue;
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

        EditorView ev = parent as EditorView;
        if (ev != null)
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
            //hovered segment has changed?
            /*if (
                (m_nearest != null) &&
                ((m_nearest.Item1 != nearest.Item1) || (m_nearest.Item2 != nearest.Item2))
                )*/
            {

                m_hoveredSegments.Clear();
                m_hoveredContour = FindContour(nearest, out m_hoveredSegments); //store new contour

                m_inside = Geom2D.IsInside(m_hoveredContour, mouseWorldPos);
                //complex contour treatment (polygon with holes)
                if (m_inside)
                {
                    /* mouse cursor is inside polygon:
                     * Identify any sements inside and find next inner contour. 
                     * Remove any segments inside of inner contour.
                     * Repeat until no inner contours and segments are left.
                     */
                    ProcessInnerSegments(m_mapData.Segments, m_hoveredContour, mouseWorldPos);
                }
                else
                {
                    /* mouse cursor is outside polygon:
                     * Identify any sements outside and find next contour. 
                     * if mouse sursor is outside polygon, repeat.
                     * if mouse cursor is inside polygon, continue with finding any remaining inner contour
                     * (details see case above).
                     */
                    ProcessOuterSegments(m_mapData.Segments, m_hoveredContour, mouseWorldPos);
                }
            }
            m_nearest = nearest;
        }
        else
        {
            m_cursorInfo.HoverRegion = null;
        }
    }

    private void ProcessInnerSegments(List<Segment> segments, List<Vector2> outerContour, Vector2 mouseWorldPos)
    {
        List<Segment> innerSegments = FindInnerSegments(segments, outerContour);
        if (innerSegments != null)
        {
            int iterations = 0;
            while (innerSegments.Count > 0 && iterations < innerSegments.Count)
            {
                Segment segment = FindNearestSegment(innerSegments);
                if (segment != null)
                {
                    bool left = Geom2D.IsCcw(segment.Vertex1.WorldPosition, segment.Vertex2.WorldPosition, mouseWorldPos);
                    Tuple<Segment, bool> nearest = new Tuple<Segment, bool>(segment, left);
                    List<Vector2> contour = FindContour(nearest, out List<Segment> contourSegments);
                    //segments inside of inner contours can never be part of the final contour - remove
                    RemoveInnerSegments(innerSegments, contour);
                    //remove inner contour itself from list of inner segments
                    contourSegments.ForEach(x => innerSegments.Remove(x));
                    //make sure inner contours are hovered as well
                    m_hoveredSegments.AddRange(contourSegments);
                }
                iterations++; //make sure deadlock can never happen
            }
        }
    }

    private void ProcessOuterSegments(List<Segment> segments, List<Vector2> innerContour, Vector2 mouseWorldPos)
    {
        List<Segment> outerSegments = FindOuterSegments(segments, innerContour);
        if (outerSegments != null)
        {
            int interations = 0;
            while (outerSegments.Count > 0 && interations < outerSegments.Count)
            {
                Segment segment = FindNearestSegment(outerSegments);
                if (segment != null)
                {
                    bool left = Geom2D.IsCcw(segment.Vertex1.WorldPosition, segment.Vertex2.WorldPosition, mouseWorldPos);
                    Tuple<Segment, bool> nearest = new Tuple<Segment, bool>(segment, left);
                    List<Vector2> contour = FindContour(nearest, out List<Segment> contourSegments);
                    //remove contour itself from list of inner segments
                    contourSegments.ForEach(x => outerSegments.Remove(x));
                    //make sure inner contours are hovered as well
                    m_hoveredSegments.AddRange(contourSegments);
                    if (!Geom2D.IsInside(contour, mouseWorldPos)) //another inner contour found
                    {
                        //segments inside of inner contours can never be part of the final contour - remove
                        RemoveInnerSegments(outerSegments, contour);
                    }
                    else //outer contour found
                    {
                        m_inside = true;
                        ProcessInnerSegments(outerSegments, contour, mouseWorldPos);
                        outerSegments.Clear(); //done
                    }
                }
                interations++; //make sure deadlock can never happen
            }
        }
    }

    Vector2 dp1, dp2;
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
            else if (sqrDist == minDist) //find best candidate of multiple segments with same distance
            {
                //the steeper the angle (--> 0) the less reliable the result is - always take bigger angle (smaller dot product)
                EditorView ev = parent as EditorView;
                if (ev != null)
                {
                    Vector2 mouseWorldPos = ev.ScreenToWorldSpace(m_mousePos);

                    Vector2 connection;
                    Vector2 current;
                    Vector2 next;
                    if (segment.Vertex2 == segments[i].Vertex1) 
                    {
                        connection = segment.Vertex2.WorldPosition;
                        current = segment.Vertex1.WorldPosition;
                        next = segments[i].Vertex2.WorldPosition;
                    }
                    else if (segment.Vertex1 == segments[i].Vertex1)
                    {
                        connection = segment.Vertex1.WorldPosition;
                        current = segment.Vertex2.WorldPosition;
                        next = segments[i].Vertex2.WorldPosition;
                    }
                    else if (segment.Vertex2 == segments[i].Vertex2)
                    {
                        connection = segment.Vertex2.WorldPosition;
                        current = segment.Vertex1.WorldPosition;
                        next = segments[i].Vertex1.WorldPosition;
                    }
                    else
                    {
                        connection = segment.Vertex1.WorldPosition;
                        current = segment.Vertex2.WorldPosition;
                        next = segments[i].Vertex1.WorldPosition;
                    }

                    Vector2 lhs = (connection - mouseWorldPos).normalized;
                    Vector2 rhscurrent = (current - connection).normalized;
                    Vector2 rhsnext = (next - connection).normalized;

                    float dotcurrent = Vector2.Dot(lhs, rhscurrent);
                    float dotnext = Vector2.Dot(lhs, rhsnext);

                    if (dotnext < dotcurrent)
                        segment = segments[i];
                }
            }
            else
            {
                //nop
            }
        }
        return segment;
    }

    private List<Segment> FindInnerSegments(List<Segment> segments, List<Vector2> contour)
    {
        List<Segment> innerSegments = new List<Segment>();
        for (int i = 0;  i < segments.Count; i++) 
        {
            Vector2 point = segments[i].Vertex1.WorldPosition + (segments[i].Vertex2.WorldPosition - segments[i].Vertex1.WorldPosition) * 0.5f;
            if (
                Geom2D.IsInside(contour, point) &&
                //Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) &&
                //Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) &&
                !m_hoveredSegments.Contains(segments[i])
                )
                innerSegments.Add(segments[i]); //TODO is this sufficient?
        }
        return innerSegments;
    }

    private List<Segment> FindOuterSegments(List<Segment> segments, List<Vector2> contour)
    {
        List<Segment> outerSegments = new List<Segment>();
        for (int i = 0; i < segments.Count; i++)
        {
            Vector2 point = segments[i].Vertex1.WorldPosition + (segments[i].Vertex2.WorldPosition - segments[i].Vertex1.WorldPosition) * 0.5f;
            if (
                !Geom2D.IsInside(contour, point) &&
                //!Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) &&
                //!Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) &&
                !m_hoveredSegments.Contains(segments[i])
                )
                outerSegments.Add(segments[i]); //TODO is this sufficient?
        }
        return outerSegments;
    }

    private void RemoveInnerSegments(List<Segment> segments, List<Vector2> contour)
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (
                (Geom2D.IsInside(contour, segments[i].Vertex1.WorldPosition) ||
                Geom2D.IsInside(contour, segments[i].Vertex2.WorldPosition) ||
                (contour.Contains(segments[i].Vertex1.WorldPosition) && contour.Contains(segments[i].Vertex2.WorldPosition)) //edge case
                ) && 
                !m_hoveredSegments.Contains(segments[i])
                )
                segments.RemoveAt(i); //TODO is this sufficient?
        }
    }

    private List<Vector2> FindContour(Tuple<Segment, bool> nearest, out List<Segment> hoveredSegments)
    {
        List<Vector2> hoveredContour = new List<Vector2>();
        hoveredSegments = new List<Segment>();
        //init
        //m_hoveredSegments.Clear();

        //iterate through connected segments
        Tuple<Segment, bool> first = nearest;
        Tuple<Segment, bool> current = first;
        do
        {
            current = ContourHelper.FindNextSegment(current);
            hoveredSegments.Add(current.Item1);
            Vector2 nextPos;
            if (current.Item2)
                nextPos = current.Item1.Vertex1.WorldPosition;
            else
                nextPos = current.Item1.Vertex2.WorldPosition;
            hoveredContour.Add(nextPos);
        } while (
            (current.Item1 != first.Item1 || current.Item2 != first.Item2) && 
            hoveredSegments.Count <= m_mapData.Segments.Count //avoid endless loop in case of some weird error
            );

        return hoveredContour;
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