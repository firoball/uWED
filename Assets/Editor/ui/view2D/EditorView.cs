using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.View2D
{
    public class EditorView : GridView
    {
        const float c_pixelsPerUnit = 1f;
        const bool c_invertYPosition = true;

        private readonly GridManipulator m_gridManipulator;
        private readonly EditorManipulator m_editorManipulator;
        private readonly EditorInterface m_interface;
        private float m_lockAngle = 15.0f; //in deg
        private bool m_enableSnapping;

        public EditorInterface Interface => m_interface;

        public EditorView()
        {
            m_enableSnapping = true;
            GridBackground grid = new GridBackground();
            m_gridManipulator = new GridManipulator(grid);
            m_editorManipulator = new EditorManipulator();
            m_interface = new EditorInterface(this, m_gridManipulator, m_editorManipulator);
            name = "EditorView";
            this.StretchToParentSize();
            this.AddManipulator(m_gridManipulator); //must be added before Zoomer setup
            SetupZoom(ContentZoomer.DefaultMinScale * 0.1f, ContentZoomer.DefaultMaxScale * 4.0f);
            m_gridManipulator.RegisterCallbacksLate(); //must be registered after Zoomer setup
            Add(grid);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(m_editorManipulator);

            //TODO: don't load transform prefs when map is new or has changed
            LoadPrefs();
            //pass defaults to all listeners
            m_interface.NotifyLockAngleListeners(m_lockAngle);
            m_interface.NotifyToggleSnappingListeners(m_enableSnapping);

            contentViewContainer.BringToFront();
            //TODO: only perform schedule.Execute when prefs were not found/loaded (e.g. new map)
            schedule.Execute(() =>
            {
                contentViewContainer.style.translate = parent.worldBound.size / 2f;
                //TODO: don't load transform prefs when map is new or has changed
                LoadPrefs();
            });
            
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        public Matrix4x4 WorldToContentMatrix()
        {
            //layout offset - this is already in screen coordinates
            Vector3 layoutTranslate =
                new Vector3(contentViewContainer.layout.position.x, contentViewContainer.layout.position.y);
            //invert y if configured
            Vector3 s = new Vector3(1, 1, 1);
            if (c_invertYPosition)
                s.y = -s.y;
            //configured pixel resolution
            Vector3 pixelScale = new Vector3(c_pixelsPerUnit, c_pixelsPerUnit);
            //build actual world to content matrix
            return Matrix4x4.Translate(layoutTranslate) * Matrix4x4.Scale(s) * Matrix4x4.Scale(pixelScale);
        }
        
        public Vector2 WorldToContentSpace(Vector2 pos)
        {
            return WorldToContentMatrix().MultiplyPoint3x4(pos);
        }

        public Matrix4x4 WorldToScreenMatrix()
        {
            return GetViewContainerMatrix() * WorldToContentMatrix();
            //layout offset - this is already in screen coordinates
            Vector3 layoutTranslate =
                new Vector3(contentViewContainer.layout.position.x, contentViewContainer.layout.position.y);
            //invert y if configured
            Vector3 s = new Vector3(1, 1, 1);
            if (c_invertYPosition)
                s.y = -s.y;
            //configured pixel resolution
            Vector3 pixelScale = new Vector3(c_pixelsPerUnit, c_pixelsPerUnit);
            //build actual world to screen matrix
            return GetViewContainerMatrix() * Matrix4x4.Translate(layoutTranslate) * Matrix4x4.Scale(s) *
                   Matrix4x4.Scale(pixelScale);
        }

        public Vector2 WorldToScreenSpace(Vector2 pos)
        {
            var position = TransformScreenPos(pos * c_pixelsPerUnit - contentViewContainer.layout.position);
            return GetViewContainerMatrix().MultiplyPoint3x4(position);
        }

        public Vector2 TransformScreenPos(Vector2 pos)
        {
            if (c_invertYPosition)
                pos.y = -pos.y;
            return pos;
        }

        public Vector2 ScreenToWorldSpace(Vector2 pos)
        {
            Vector2 position = TransformScreenPos(GetViewContainerMatrix().inverse.MultiplyPoint3x4(pos));
            return (position + contentViewContainer.layout.position) / c_pixelsPerUnit;
        }

        public float ScaleScreenToWorld(float length)
        {
            return length / contentViewContainer.resolvedStyle.scale.value.x / c_pixelsPerUnit;
        }

        public Vector2 SnapWorldPos(Vector2 pos)
        {
            if (m_enableSnapping)
            {
                Vector2 fac = pos / m_gridManipulator.GridSpacing;
                int snapX = (fac.x < 0) ? (int)(fac.x - 0.5f) : (int)(fac.x + 0.5f);
                int snapY = (fac.y < 0) ? (int)(fac.y - 0.5f) : (int)(fac.y + 0.5f);
                Vector2 intfac = new Vector2(snapX, snapY);
                return intfac * m_gridManipulator.GridSpacing;
            }
            else
            {
                return pos;
            }
        }

        public Vector2 SnapScreenPos(Vector2 pos)
        {
            if (m_enableSnapping)
            {
                Vector2 worldPos = ScreenToWorldSpace(pos);
                Vector2 snappedPos = SnapWorldPos(worldPos);
                return WorldToScreenSpace(snappedPos);
            }
            else
            {
                return pos;
            }
        }

        public float SnapAngle(float angle) //angle in rad
        {
            if (m_enableSnapping)
            {
                float degrees = angle * 180 / Mathf.PI; //angle in deg
                int snapped;
                if (degrees > 0.0f)
                    snapped = (int)((degrees + 0.5f * m_lockAngle) / m_lockAngle);
                else
                    snapped = (int)((degrees - 0.5f * m_lockAngle) / m_lockAngle);
                angle = snapped * m_lockAngle;
                if (angle <= -180f) angle = 180f; //edge case: -180 deg -> 180 deg
                return (angle / 180) * Mathf.PI; //angle in rad
            }
            else
            {
                return angle; //angle in rad
            }
        }

        public void ToggleSnapping(bool enable)
        {
            m_enableSnapping = enable;
            m_interface.NotifyToggleSnappingListeners(m_enableSnapping);
        }

        public void LockAngle(float angle)
        {
            m_lockAngle = angle;
            m_interface.NotifyLockAngleListeners(m_lockAngle);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.C) CenterView();
            if (evt.keyCode == KeyCode.F) FitViewToWindow();
        }
        
        public void FitViewToWindow()
        {
            Vector2 minTrans = WorldToContentSpace(m_editorManipulator.GetMapMin());
            Vector2 maxTrans = WorldToContentSpace(m_editorManipulator.GetMapMax());
            
            Vector2 min = Vector2.Min(minTrans, maxTrans);
            Vector2 max = Vector2.Max(minTrans, maxTrans);
            Zoomer.FrameToFit(min, max, 0.95f);
        }

        public void CenterView()
        {
            Vector2 minTrans = WorldToContentSpace(m_editorManipulator.GetMapMin());
            Vector2 maxTrans = WorldToContentSpace(m_editorManipulator.GetMapMax());
            
            Vector2 min = Vector2.Min(minTrans, maxTrans);
            Vector2 max = Vector2.Max(minTrans, maxTrans);
            Zoomer.ApplyFrame(min, max);
        }
        
        public void SavePrefs()
        {
            m_gridManipulator?.SavePrefs();
            m_editorManipulator?.SavePrefs();
            EditorPrefs.SetBool("uWED::EditorView::enableSnapping", m_enableSnapping);
            EditorPrefs.SetFloat("uWED::EditorView::lockAngle", m_lockAngle);
            EditorPrefs.SetFloat("uWED::EditorView::transform.position.x",
                contentViewContainer.resolvedStyle.translate.x);
            EditorPrefs.SetFloat("uWED::EditorView::transform.position.y",
                contentViewContainer.resolvedStyle.translate.y);
            EditorPrefs.SetFloat("uWED::EditorView::transform.scale.x",
                contentViewContainer.resolvedStyle.scale.value.x);
            EditorPrefs.SetFloat("uWED::EditorView::transform.scale.y",
                contentViewContainer.resolvedStyle.scale.value.y);
        }

        private void LoadPrefs()
        {
            if (EditorPrefs.HasKey("uWED::EditorView::enableSnapping"))
                m_enableSnapping = EditorPrefs.GetBool("uWED::EditorView::enableSnapping");
            if (EditorPrefs.HasKey("uWED::EditorView::lockAngle"))
                m_lockAngle = EditorPrefs.GetFloat("uWED::EditorView::lockAngle");

            //TODO: load pos and scale only if map has not changed
            Vector3 pos = contentViewContainer.resolvedStyle.translate;
            if (EditorPrefs.HasKey("uWED::EditorView::transform.position.x"))
                pos.x = EditorPrefs.GetFloat("uWED::EditorView::transform.position.x");
            if (EditorPrefs.HasKey("uWED::EditorView::transform.position.y"))
                pos.y = EditorPrefs.GetFloat("uWED::EditorView::transform.position.y");
            contentViewContainer.style.translate = pos;

            Vector3 scale = contentViewContainer.resolvedStyle.scale.value;
            if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.x"))
                scale.x = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.x");
            if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.y"))
                scale.y = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.y");
            contentViewContainer.style.scale = scale;
        }

        private Matrix4x4 GetViewContainerMatrix()
        {
            Vector3 t = contentViewContainer.resolvedStyle.translate;
            Quaternion r = Quaternion.Euler(contentViewContainer.resolvedStyle.rotate.angle.ToDegrees(), 0, 0);
            Vector3 s = contentViewContainer.resolvedStyle.scale.value;

            //build actual world to screen matrix
            return Matrix4x4.TRS(t, r, s);
        }
    }
}