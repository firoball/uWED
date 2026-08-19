using System.Collections;
using System.Collections.Generic;
using Editor.Drawers;
using UnityEngine;

namespace Editor.Modes
{
    public class ObjectMode : BaseEditorMode
    {

        private MapObject m_currentObject;
        private bool m_rotate;

        public ObjectMode(MapData mapData) : base(mapData, new ObjectDrawer(mapData))
        {
        }

        public override void Initialize()
        {
            m_currentObject = null;
            m_rotate = false;

            base.Initialize();
        }

        public override bool StartDrag(bool alt)
        {
            CursorInfo ci = m_drawer.CursorInfo;
            if (ci.HoverObject != null)
            {
                m_currentObject = ci.HoverObject;
                m_drawer.SetDragMode(true, alt);
                m_rotate = alt;
                return true;
            }

            return false;
        }

        public override void FinishDrag(Vector2 mouseSnappedWorldPos)
        {
            if (m_currentObject != null && !m_rotate)
            {
                m_currentObject.Vertex.WorldPosition = mouseSnappedWorldPos;
            }

            m_drawer.SetDragMode(false, m_rotate);
            m_currentObject = null;
        }

        public override void AbortDrag()
        {
            m_drawer.SetDragMode(false, false);
            m_currentObject = null;
        }

        public override bool StartConstruction(Vector2 mouseSnappedWorldPos)
        {
            CursorInfo ci = m_drawer.CursorInfo;
            if (ci.HoverObject != null) //start from hovered object not allowed
            {
                return true; //done
            }
            else if (ci.NearObject != null) //start from nearby object not allowed
            {
                return true; //done
            }
            else //place new object
            {
                m_mapData.Add(new MapObject(mouseSnappedWorldPos));
                return true; //construction immediately finished
            }

        }

        public override void DeleteObject()
        {
            CursorInfo ci = m_drawer.CursorInfo;

            if (ci.SelectedObjects.Count > 0) //delete marked objects
            {
                foreach (MapObject o in ci.SelectedObjects)
                    m_mapData.Remove(o);
                m_drawer.Unselect();
            }
            else if (ci.HoverObject != null) //object is hovered - delete it
            {
                m_mapData.Remove(ci.HoverObject);
            }
        }

    }
}