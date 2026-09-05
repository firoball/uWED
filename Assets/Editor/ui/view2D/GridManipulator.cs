using Runtime.Platform;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.View2D
{
    public class GridManipulator : MouseManipulator
    {
        private readonly GridBackground m_grid;
        private float m_gridScale = 5f; //2^m_gridScale must be m_gridSpacing
        private float m_gridSpacing = 32f; //must match GridBackground --spacing:
        private bool m_gridEnabled = true;

        public float GridSpacing
        {
            get => m_gridSpacing;
            set => m_gridSpacing = value;
        }

        private const float c_minGridScale = 1f;
        private const float c_maxGridScale = 10f;
        private const float c_gridVisibilityThresholdPx = 5f;

        public GridManipulator(GridBackground grid)
        {
            m_grid = grid;
            EditorEventBus.Instance.LoadPrefs.Subscribe(OnLoadPrefs);
            EditorEventBus.Instance.SavePrefs.Subscribe(OnSavePrefs);
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<WheelEvent>(OnWheel);
            //pass defaults to all listeners - OnCustomStyleResolved is too late for init phase
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
            target.UnregisterCallback<WheelEvent>(OnWheelLate); //does this break things?
            EditorEventBus.Instance.ZoomChanged.Unsubscribe(OnZoomChanged);
        }

        public void RegisterCallbacksLate()
        {
            target.RegisterCallback<WheelEvent>(OnWheelLate);
            EditorEventBus.Instance.ZoomChanged.Subscribe(OnZoomChanged);
        }

        public void ToggleGrid(bool enable)
        {
            m_gridEnabled = enable;
            EditorEventBus.Instance.ToggleGrid.Raise(m_gridEnabled);
            UpdateBackground();
        }

        public void ScaleGrid(float scale)
        {
            m_gridScale = Mathf.Clamp(scale, c_minGridScale, c_maxGridScale);
            EditorEventBus.Instance.ScaleGrid.Raise(m_gridScale);

            float spacing = (1 << (int)m_gridScale);
            m_grid.Spacing = spacing;
            m_gridSpacing = spacing;
            UpdateBackground();
        }

        private void OnWheel(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                evt.StopImmediatePropagation(); //disable zoomer
                if (evt.delta.y != 0)
                {
                    float scale = m_gridScale + Mathf.Sign(evt.delta.y);
                    ScaleGrid(scale);
                }
            }
        }

        private void OnWheelLate(WheelEvent evt)
        {
            UpdateBackground();
        }

        private void OnZoomChanged(bool zoomedIn)
        {
            UpdateBackground();
        }
        
        private void HideBackground()
        {
            m_grid.EnableDraw = false;
        }

        private void ShowBackground()
        {
            m_grid.EnableDraw = true;
        }

        private void UpdateBackground()
        {
            if (m_gridEnabled)
            {
                if (target is EditorView editorView)
                {
                    Vector2 v1 = editorView.WorldToScreenSpace(new Vector2(m_gridSpacing, 0));
                    Vector2 v2 = editorView.WorldToScreenSpace(new Vector2(0, 0));

                    if ((v1 - v2).x < c_gridVisibilityThresholdPx)
                        HideBackground();
                    else
                        ShowBackground();
                }
                else
                {
                    ShowBackground();
                }
            }
            else
            {
                HideBackground();
            }
        }

        private void OnSavePrefs(IPrefsProvider prefsProvider)
        {
            if (prefsProvider == null)
            {
                Debug.LogWarning("GridManipulator.OnSavePrefs: no IPrefsProvider set, skipping save.");
                return;
            }

            prefsProvider.SetFloat("uWED::GridManipulator::gridScale", m_gridScale);
            prefsProvider.SetBool("uWED::GridManipulator::gridEnabled", m_gridEnabled);
        }

        private void OnLoadPrefs(IPrefsProvider prefsProvider)
        {
            if (prefsProvider == null)
            {
                Debug.LogWarning("GridManipulator.OnLoadPrefs: no IPrefsProvider set, skipping load.");
                return;
            }

            float gridScale = prefsProvider.GetFloat("uWED::GridManipulator::gridScale", m_gridScale);
            bool gridEnabled = prefsProvider.GetBool("uWED::GridManipulator::gridEnabled", m_gridEnabled);

            ToggleGrid(gridEnabled);
            ScaleGrid(gridScale);
        }

    }
}