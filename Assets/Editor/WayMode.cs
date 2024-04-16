using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayMode : IEditorMode
{
    private MapDrawer m_drawer;
    private MapData m_mapData;

    public WayMode(MapData mapData, MapDrawer drawer)
    {
        m_drawer = drawer;
        m_mapData = mapData;
    }

    public void StartDrag(CursorInfo ci)
    {
    }

    public void FinishDrag(CursorInfo ci, Vector2 mouseSnappedWorldPos)
    {
    }

    public bool StartConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos)
    {
        return true; //construction finished
    }

    public bool RevertConstruction()
    {
        return true; //construction finished
    }

    public bool ProgressConstruction(CursorInfo ci, Vector2 mouseSnappedWorldPos)
    {
        return true; //construction finished
    }

    public void EditObject(CursorInfo ci)
    {
    }

    public void DeleteObject(CursorInfo ci)
    {
    }

    public void ModifyObject(CursorInfo ci, Vector2 mouseWorldPos, EditorView ev)
    {
    }

    public void ModifyObjectAlt(CursorInfo ci, Vector2 mouseWorldPos, EditorView ev)
    {
    }

}
