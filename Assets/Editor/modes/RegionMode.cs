using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionMode : BaseEditorMode
{

    public RegionMode(MapData mapData) : base(mapData, new RegionDrawer(mapData))
    {
    }

    public override void Initialize()
    {
        UpdateBoundaries(); //rebuild region min/max boundaries
        base.Initialize();
    }

    public override bool StartConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverContour == null)
        {
            return true; //done
        }
        else
        {
            //Get region reference
            Region region = ci.HoverRegion; //update existing (potentially broken) region
            if (ci.HoverRegion == null) //create new region
            {
                region = new Region();
                m_mapData.Add(region);
            }

            /* identify any segment referencing currently hovered counter and
             * assign the region reference to corresponding segment side
             * 
             * in case the region is still assigned to a segment which is not part of 
             * the hovered contour, this must be a leftover of some editing activity
             * and therefore can be removed
             */
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < m_mapData.Segments.Count; i++)
            {
                Segment s = m_mapData.Segments[i];

                //remove possible region leftovers from segments
                if (!ci.HoverContour.Is(s.CLeft) && s.Left == region) //left side
                    s.Left = null;
                if (!ci.HoverContour.Is(s.CRight) && s.Right == region) //right side
                    s.Right = null;

                //assign region to segments
                if (ci.HoverContour.Is(s.CLeft) && s.Left != region) //left side
                {
                    s.Left = region;
                    //identify region min/max boundaries
                    SetMinMax(s.Vertex2.WorldPosition, ref min, ref max);
                }
                if (ci.HoverContour.Is(s.CRight) && s.Right != region) //right side
                {
                    s.Right = region;
                    //identify region min/max boundaries
                    SetMinMax(s.Vertex1.WorldPosition, ref min, ref max);
                }
            }
            region.Min = min;
            region.Max = max;

            m_drawer.MarkDirtyRepaint();
            return true; //construction immediately finished
        }

    }


    public override void EditObject()
    {
        //TODO: implement
    }

    public override void DeleteObject()
    {
        CursorInfo ci = m_drawer.CursorInfo;

        if (ci.SelectedRegions.Count > 0)
        {
            foreach (Region r in ci.SelectedRegions)
                DeleteRegion(r);
            m_drawer.Unselect();
        }
        else if (ci.HoverRegion != null) //region is hovered - destroy it
        {
            DeleteRegion(ci.HoverRegion);
        }
        else
        {
            //nothing do destroy
        }
    }

    private void DeleteRegion(Region r)
    {
        //remove all references
        for (int s = 0; s < m_mapData.Segments.Count; s++) 
        {
            if (m_mapData.Segments[s].Left == r)
                m_mapData.Segments[s].Left = null;
            if (m_mapData.Segments[s].Right == r)
                m_mapData.Segments[s].Right = null;
        }
        //remove region
        m_mapData.Remove(r);
    }

    private void UpdateBoundaries()
    {
        //reset region min/max boundaries
        foreach(Region r in m_mapData.Regions)
        {
            r.Min = new Vector2(float.MaxValue, float.MaxValue);
            r.Max = new Vector2(float.MinValue, float.MinValue);
        }

        //recalculate region min/max boundaries
        for (int i = 0; i < m_mapData.Segments.Count; i++)
        {
            Segment s = m_mapData.Segments[i];
            if (s.Right != null)
            {
                Region r = s.Right;
                Vector2 min = r.Min;
                Vector2 max = r.Max;
                SetMinMax(s.Vertex1.WorldPosition, ref min, ref max);
                r.Min = min; 
                r.Max = max;
            }

            if (s.Left != null)
            {
                Region r = s.Left;
                Vector2 min = r.Min;
                Vector2 max = r.Max;
                SetMinMax(s.Vertex2.WorldPosition, ref min, ref max);
                r.Min = min;
                r.Max = max;
            }
        }
    }

    private void SetMinMax(Vector2 pos, ref Vector2 min, ref Vector2 max)
    {
        min.x = Mathf.Min(min.x, pos.x);
        max.x = Mathf.Max(max.x, pos.x);
        min.y = Mathf.Min(min.y, pos.y);
        max.y = Mathf.Max(max.y, pos.y);
    }

}
