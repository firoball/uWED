using System.Collections.Generic;
using UnityEngine;

public class ObjectDrawer : BaseEditorDrawer
{
    private Color c_objectDragColor = new Color(0.0f, 0.3f, 0.0f, 1.0f);

    //Dragging mode
    private bool m_dragging;
    private bool m_rotating;
    private MapObject m_draggedObject;
    //private List<MapObject> m_draggedObjects;

    public ObjectDrawer(MapData mapData) : base(mapData) 
    {
        m_enableObjectDetails = true;
    }

    public override void Initialize()
    {
        m_dragging = false;
        m_rotating = false;
        m_draggedObject = null;
        //m_draggedObjects = new List<MapObject>();

        base.Initialize();
    }

    public override void SetSelectSingle()
    {
        if (m_cursorInfo.HoverObject != null)
        {
            MapObject o = m_cursorInfo.HoverObject;
            if (!m_cursorInfo.SelectedObjects.Contains(o))
                m_cursorInfo.SelectedObjects.Add(o);
            else
                m_cursorInfo.SelectedObjects.Remove(o);
        }
    }

    public override void Unselect()
    {
        m_cursorInfo.SelectedObjects.Clear();
    }

    public override void SetDragMode(bool on, bool alt)
    {
        //if alt is true, object rotation is activated
        //on valid finish alt is set true as well
        //on abort alt is set to false
        m_dragging = false;
        if (on && m_cursorInfo.HoverObject != null)
        {
            m_dragging = true;
            m_rotating = alt;
            m_draggedObject = m_cursorInfo.HoverObject;
        }
        else
        {
            if (m_rotating && alt) //normally assignments should not be done in drawer, but only here all params are available
                m_draggedObject.Angle = CalcMouseAngle(m_draggedObject.Vertex.ScreenPosition);

            m_rotating = false;
            m_draggedObject = null;
        }
    }

    protected override void  SelectMultiple(Rect selection)
    {
        foreach (MapObject o in m_mapData.Objects)
        {
            if (!m_cursorInfo.SelectedObjects.Contains(o) && selection.Contains(o.Vertex.ScreenPosition))
                m_cursorInfo.SelectedObjects.Add(o);
        }
    }


    protected override Color SetObjectColor(int i)
    {
        Color color;
        if (m_dragging && m_mapData.Objects[i] == m_draggedObject)
        {
            if (m_rotating)
                color = c_hoverColor;
            else
                color = c_objectDragColor;
        }
        else if (!m_dragging && m_mapData.Objects[i] == m_cursorInfo.HoverObject)
            color = c_hoverColor;
        else if (m_cursorInfo.SelectedObjects.Contains(m_mapData.Objects[i]))
            color = c_selectColor;
        else
            color = base.SetObjectColor(i);

        return color;
    }

    protected override float SetObjectAngle(int i)
    {
        if (m_rotating && m_mapData.Objects[i] == m_draggedObject)
        {
            return CalcMouseAngle(m_draggedObject.Vertex.ScreenPosition);
        }
        else
        { 
            return base.SetObjectAngle(i);
        }
    }

    protected override void ImmediateRepaint()
    {
        if (!enabledSelf)
            return;

        base.ImmediateRepaint();

        DrawModes();
    }

    protected override void HoverTest()
    {
        int pointHoverSize = 16;
        MapObject mapObject = null;
        MapObject snappedMapObject = null;
        int halfSize = (pointHoverSize - 1) / 2;

        for (int o = 0; o < m_mapData.Objects.Count && mapObject == null; o++)
        {
            Vector2 pos = m_mapData.Objects[o].Vertex.ScreenPosition;
            Rect rect = new Rect(pos.x - halfSize, pos.y - halfSize, pointHoverSize, pointHoverSize);
            if (rect.Contains(m_mousePos))
            {
                mapObject = m_mapData.Objects[o];
            }
            else if (rect.Contains(m_mouseSnappedPos))
            {
                snappedMapObject = m_mapData.Objects[o];
            }
            else
            {
                //nothing to do
            }
        }

        m_cursorInfo.HoverObject = mapObject;
        m_cursorInfo.NearObject = snappedMapObject;
    }

    private void DrawModes()
    {
        //Draw mouse - drag mode
        if (m_dragging && m_draggedObject != null)
        {
            if (m_rotating)
            {
                EditorView ev = parent as EditorView;
                if (ev != null)
                {
                    float length = (m_mousePos - m_draggedObject.Vertex.ScreenPosition).magnitude - c_bigObjectSize;
                    float angle = CalcMouseAngle(m_draggedObject.Vertex.ScreenPosition);
                    Vector2 p1;
                    p1.x = length * Mathf.Cos(angle);
                    p1.y = length * Mathf.Sin(angle);
                    Vector2 p2 = p1.normalized * c_bigObjectSize;
                    p1 += m_draggedObject.Vertex.ScreenPosition;
                    p2 += m_draggedObject.Vertex.ScreenPosition;

                    DrawLine(p2, p1, c_validColor);
                }
            }
            else
            {
                DrawCircle(m_mouseSnappedPos, c_validColor, c_bigObjectSize);
                DrawArrow(m_mouseSnappedPos, c_objectBgColor, c_bigObjectSize, m_draggedObject.Angle);
            }
        }

    }

    private float CalcMouseAngle(Vector2 screenPosition)
    { 
        EditorView ev = parent as EditorView;
        if (ev != null)
        {
            Vector2 v = m_mousePos - screenPosition;
            return ev.SnapAngle(Mathf.Atan2(v.y, v.x));
        }
        return 0.0f;
    }

}