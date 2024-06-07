using UnityEngine;
using UnityEngine.UIElements;

public abstract class BaseEditorMode
{
    protected readonly BaseEditorDrawer m_drawer;
    protected MapData m_mapData;

    public BaseEditorDrawer Drawer => m_drawer;

    public BaseEditorMode(MapData mapData, BaseEditorDrawer drawer)
    {
        drawer.SetEnabled(false);
        m_drawer = drawer;
        m_mapData = mapData;

        Initialize();
    }

    public virtual void Initialize()
    {
        m_drawer?.Initialize();
    }

    public virtual bool StartDrag(bool alt)
    {
        //evaluate "alt" for alternative drag routine (ctrl key pressed)
        return false;
    }

    public virtual void FinishDrag(Vector2 mouseSnappedWorldPos) { }

    public virtual void AbortDrag() { }

    public virtual bool StartConstruction(Vector2 mouseSnappedWorldPos)
    {
        return true; //started and already finished
    }

    public virtual bool RevertConstruction()
    {
        return true; //fully reverted
    }

    public virtual bool ProgressConstruction(Vector2 mouseSnappedWorldPos)
    {
        return true; //finished
    }

    public virtual void EditObject() { }
    public virtual void DeleteObject() { }
    public virtual void ModifyObject(Vector2 mouseWorldPos, EditorView ev) { }
    public virtual void ModifyObjectAlt(Vector2 mouseWorldPos, EditorView ev) { }

    public virtual void StartSelection()
    {
        m_drawer?.SetSelectMode(true);
    }

    public virtual void FinishSelection()
    {
        m_drawer?.SetSelectMode(false);
    }

    public virtual void SingleSelection()
    {
        m_drawer?.SetSelectSingle();
    }

    public virtual bool ClearSelection()
    {
        if (m_drawer == null)
            return false; //there can't be any selection active

        bool selected = m_drawer.IsSelectionActive();
        m_drawer.Unselect();
        return selected;
    }

}