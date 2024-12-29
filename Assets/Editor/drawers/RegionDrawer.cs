using System;
using System.Collections.Generic;
using UnityEngine;

public class RegionDrawer : BaseEditorDrawer
{
    private Color c_lineDarkColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    public RegionDrawer(MapData mapData) : base(mapData)
    {
        //m_enableNormals = true; //temp
    }

    public override void Initialize()
    {
        FindContours(); //TEMP
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
            color = c_hoverColor;*/
        /*else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && m_nearest.Item2) //TODO: temp - for testing only
            color = Color.white;
        else if (m_nearest != null && m_nearest.Item1 == m_mapData.Segments[i] && !m_nearest.Item2) //TODO: temp - for testing only
            color = Color.blue;
        */
        /*else*/ if (m_cursorInfo.SelectedRegions.Contains(m_mapData.Segments[i].Left) || m_cursorInfo.SelectedRegions.Contains(m_mapData.Segments[i].Right))
            color = c_selectColor;
        else if (m_cursorInfo.HoverContour != null && //segment is part of contour
                (m_cursorInfo.HoverContour.Is(m_mapData.Segments[i].CLeft) || m_cursorInfo.HoverContour.Is(m_mapData.Segments[i].CRight)) 
                )
            color = c_validColor; //TODO: proper coloring
        else if (m_mapData.Segments[i].Left != null && m_mapData.Segments[i].Right != null) //two regions assigned
            color = c_lineDarkColor;
        else if (m_mapData.Segments[i].Left != null || m_mapData.Segments[i].Right != null) //one region assigned
            color = c_lineColor;
        else //no region created yet
            color = c_invalidColor;

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

                //mouse is hovering an "inner" contour - hover outer contour + all contained "inner" contours instead
                if (hoverContour != null && hoverContour.IsInner)
                    hoverContour = hoverContour.Outer;
                m_cursorInfo.HoverContour = hoverContour;
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
        m_mapData.Contours.Clear();

        //Step 1: Clear previous assignments
        for (int s = 0; s < m_mapData.Segments.Count; s++)
        {
            m_mapData.Segments[s].CLeft = null;
            m_mapData.Segments[s].CRight = null;
        }

        //Step2: Build all contours
        for (int s = 0; s < m_mapData.Segments.Count; s++)
        {
            if (m_mapData.Segments[s].CLeft == null)
                FindContour(m_mapData.Segments[s], true, inners);
            if (m_mapData.Segments[s].CRight == null)
                FindContour(m_mapData.Segments[s], false, inners);
        }

        //Step3: Attach inner contours to their outer contours
        for (int c = 0; c < m_mapData.Contours.Count; c++) 
        {
            ContourHelper.FindInnerContour(m_mapData.Contours[c], inners);
        }
    }

    private void FindContour(Segment segment, bool left, List<Contour> inners)
    {
        Tuple<Segment, bool> t = new Tuple<Segment, bool>(segment, left);
        Contour c = ContourHelper.FindContour(t, m_mapData.Segments.Count);
        if (c != null)
        {
            if (c.IsInner)
                inners.Add(c);
            else
                m_mapData.Contours.Add(c);
        }
    }
}