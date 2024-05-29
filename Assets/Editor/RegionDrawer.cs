using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RegionDrawer : BaseEditorDrawer
{
    private List<Tuple<Segment, bool>> m_hoveredSegments; //TODO: make local and use cursorInfo only
    private List<List<Vector2>> m_hoveredContours;
    private Tuple<Segment, bool> m_nearest;
    private bool m_inside;
    private Color c_lineDarkColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    public RegionDrawer(MapData mapData) : base(mapData)
    {
        //m_enableNormals = true; //temp
    }

    public override void Initialize()
    {
        m_hoveredSegments = new List<Tuple<Segment, bool>>();
        m_hoveredContours = new List<List<Vector2>>(); //TODO: is this really useful?
        m_nearest = null;
        m_inside = false;
        base.Initialize();
    }

    protected override Color SetSegmentColor(int i)
    {
        Color color;
        if ((m_cursorInfo.HoverRegion != null) &&
            (m_mapData.Segments[i].Left == m_cursorInfo.HoverRegion || m_mapData.Segments[i].Right == m_cursorInfo.HoverRegion)
            )//existing region
            color = c_hoverColor;
        /*else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && m_nearest.Item2) //TODO: temp - for testing only
            color = Color.white;
        else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && !m_nearest.Item2) //TODO: temp - for testing only
            color = Color.blue;
        */
        else
        {
            bool newAndHovered = (m_hoveredSegments.Where(x => x.Item1 == m_mapData.Segments[i]).FirstOrDefault() != null);
            if (newAndHovered/* && m_inside*/) //candidate for new reion
                color = c_validColor; //TODO: proper coloring
            //else if (newAndHovered && !m_inside) //candidate for border region
                //color = c_hoverColor; //TODO: proper coloring
            else if (m_mapData.Segments[i].Left != null && m_mapData.Segments[i].Right != null) //two regions assigned
                color = c_lineDarkColor;
            else if (m_mapData.Segments[i].Left != null || m_mapData.Segments[i].Right != null) //one region assigned
                color = c_lineColor;
            else //no region created yet
                color = c_invalidColor;
        }

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
    }

    protected override void HoverTest()
    {

        EditorView ev = parent as EditorView;
        if (ev != null)
        {
            Vector2 mouseWorldPos = ev.ScreenToWorldSpace(m_mousePos);

            Tuple<Segment, bool> nearest = SegmentHelper.FindNearestSegment(m_mapData.Segments, mouseWorldPos);
            FindNearestContour(mouseWorldPos);
            m_cursorInfo.HoverSegments = m_hoveredSegments;

            if (nearest.Item2) //left sided segment
                m_cursorInfo.HoverRegion = nearest.Item1.Left; //TODO: support creation of regions
            else
                m_cursorInfo.HoverRegion = nearest.Item1.Right; //TODO: support creation of regions
        }
        else
        {
            m_cursorInfo.HoverRegion = null;
            m_cursorInfo.HoverSegments.Clear();
        }
    }

    private void FindNearestContour(Vector2 worldPos)
    {
        Tuple<Segment, bool> nearest = SegmentHelper.FindNearestSegment(m_mapData.Segments, worldPos);
        //find complete contour independently from assigned regions
        //hovered segment has changed?
        if (
            (m_nearest != null) &&
            ((m_nearest.Item1 != nearest.Item1) || (m_nearest.Item2 != nearest.Item2))
            )
        {
            m_hoveredSegments.Clear();
            m_hoveredContours.Clear();
            List<Vector2> contour = ContourHelper.FindContour(nearest, out m_hoveredSegments, m_mapData.Segments.Count); //store new contour
            m_hoveredContours.Add(contour);

            m_inside = Geom2D.IsInside(contour, worldPos);
            //complex contour treatment (polygon with holes)
            if (m_inside)
            {
                /* mouse cursor is inside polygon:
                 * Identify any sements inside and find next inner contour. 
                 * Remove any segments inside of inner contour.
                 * Repeat until no inner contours and segments are left.
                 */
                ProcessInnerSegments(m_mapData.Segments, contour);
            }
            else
            {
                /* mouse cursor is outside polygon:
                 * Identify any sements outside and find next contour. 
                 * if mouse sursor is outside polygon, repeat.
                 * if mouse cursor is inside polygon, continue with finding any remaining inner contour
                 * (details see case above).
                 */
                ProcessOuterSegments(m_mapData.Segments, contour);
            }

        }
        m_nearest = nearest;
    }

    private void ProcessInnerSegments(List<Segment> segments, List<Vector2> outerContour)
    {
        List<Segment> innerSegments = SegmentHelper.FindInnerSegments(segments, outerContour, m_hoveredSegments);
        if (innerSegments != null)
        {
            int iterations = 0;
            while (innerSegments.Count > 0 && iterations < innerSegments.Count)
            {
                Vector2 point = outerContour[0]; //search more segments starting from contour start
                Tuple<Segment, bool> nearest = SegmentHelper.FindNearestSegment(innerSegments, point);
                if (nearest != null)
                {
                    List<Vector2> contour = ContourHelper.FindContour(nearest, out List<Tuple<Segment, bool>> contourSegments, m_mapData.Segments.Count);
                    m_hoveredContours.Add(contour);
                    //segments inside of inner contours can never be part of the final contour - remove
                    SegmentHelper.RemoveInnerSegments(innerSegments, contour, m_hoveredSegments);
                    //remove inner contour itself from list of inner segments
                    contourSegments.ForEach(x => innerSegments.Remove(x.Item1));
                    //make sure inner contours are hovered as well
                    m_hoveredSegments.AddRange(contourSegments);
                }
                iterations++; //make sure deadlock can never happen
            }
        }
    }

    private void ProcessOuterSegments(List<Segment> segments, List<Vector2> innerContour)
    {
        List<Segment> outerSegments = SegmentHelper.FindOuterSegments(segments, innerContour, m_hoveredSegments);
        if (outerSegments != null)
        {
            int iterations = 0;
            while (outerSegments.Count > 0 && iterations < outerSegments.Count)
            {
                Vector2 point = innerContour[0]; //search more segments starting from contour start

                Tuple<Segment, bool> nearest = SegmentHelper.FindNearestSegment(outerSegments, point);
                if (nearest != null)
                {
                    List<Vector2> contour = ContourHelper.FindContour(nearest, out List<Tuple<Segment, bool>> contourSegments, m_mapData.Segments.Count);
                    //remove contour itself from list of outer segments
                    contourSegments.ForEach(x => outerSegments.Remove(x.Item1));
                    //make sure additional contours are hovered as well
                    m_hoveredSegments.AddRange(contourSegments);
                    if (!Geom2D.IsInside(contour, point)) //another inner contour found
                    {
                        m_hoveredContours.Add(contour);
                        //segments inside of inner contours can never be part of the final contour - remove
                        SegmentHelper.RemoveInnerSegments(outerSegments, contour, m_hoveredSegments);
                    }
                    else //outer contour found
                    {
                        //outer counter must be first one in contour list
                        m_hoveredContours.Insert(0, contour);
                        m_inside = true;
                        ProcessInnerSegments(outerSegments, contour);
                        outerSegments.Clear(); //done
                    }
                }
                iterations++; //make sure deadlock can never happen
            }
        }
    }

}