using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RegionMode : BaseEditorMode
{

    public RegionMode(MapData mapData) : base(mapData, new RegionDrawer(mapData))
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        //BuildRegions();
    }

    public override bool StartConstruction(Vector2 mouseSnappedWorldPos)
    {
        CursorInfo ci = m_drawer.CursorInfo;
        if (ci.HoverRegion != null) //TODO: fix broken regions
        {
            return true; //done
        }
        else
        {
            //Add region
            Region region = new Region();
            m_mapData.Add(region);

            //reference region
            for(int i = 0; i < ci.HoverSegments.Count; i++)
            {
                if (ci.HoverSegments[i].Item2) //left side
                    ci.HoverSegments[i].Item1.Left = region;
                else //right side
                    ci.HoverSegments[i].Item1.Right = region;
            }
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

}
