using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridManipulator : MouseManipulator
{
    FlexibleGridBackground m_grid;
    private float m_gridScale = 5f; //2^m_gridScale must be m_gridSpacing
    private float m_gridSpacing = 32f; //must match GridBackground --spacing:
    private Color m_lineColor;
    private Color m_thickLineColor;
    private bool m_gridVisible = true;
    private bool m_gridEnabled = true;

    public float GridSpacing { get => m_gridSpacing; set => m_gridSpacing = value; }

    const float c_minGridScale = 1f;
    const float c_maxGridScale = 10f;

    public GridManipulator(FlexibleGridBackground grid)
    {
        m_grid = grid;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        //pass defaults to all listeners
        EditorView ev = target as EditorView;
        ev.Interface.NotifyScaleGridListeners(m_gridScale);
        ev.Interface.NotifyToggleGridListeners(m_gridEnabled);

        target.RegisterCallback<WheelEvent>(OnWheel);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<WheelEvent>(OnWheel);
        target.UnregisterCallback<WheelEvent>(OnWheelLate); //does this break things?
    }

    public void RegisterCallbacksLate()
    {
        target.RegisterCallback<WheelEvent>(OnWheelLate);
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

    public void ToggleGrid(bool enable)
    {
        m_gridEnabled = enable;
        EditorView ev = target as EditorView;
        ev?.Interface.NotifyToggleGridListeners(m_gridEnabled);
        UpdateBackground();
    }

    private void OnWheelLate(WheelEvent evt)
    {
        UpdateBackground();
    }

    public void ScaleGrid(float scale)
    {
        float oldScale = m_gridScale;
        m_gridScale = Mathf.Clamp(scale, c_minGridScale, c_maxGridScale);
        if (m_gridScale != oldScale)
        {
            EditorView ev = target as EditorView;
            ev?.Interface.NotifyScaleGridListeners(m_gridScale);
        }
        float spacing = (1 << (int)m_gridScale);
        m_grid.spacing = spacing;
        m_gridSpacing = spacing;
        UpdateBackground();
    }

    private void HideBackground()
    {
        m_gridVisible = false;
        m_lineColor = m_grid.lineColor;
        m_thickLineColor = m_grid.thickLineColor;
        Color bg = m_grid.gridBackgroundColor;

        m_grid.lineColor = bg;
        m_grid.thickLineColor = bg;
        m_grid.spacing = 10000f; //something big - renders faster
    }

    private void ShowBackground()
    {
        m_gridVisible = true;
        m_grid.lineColor = m_lineColor;
        m_grid.thickLineColor = m_thickLineColor;
        m_grid.spacing = m_gridSpacing;
    }

    private void UpdateBackground()
    {
        if (m_gridEnabled)
        {
            EditorView editorView = target as EditorView;
            Vector2 v1 = editorView.WorldtoScreenSpace(new Vector2(m_gridSpacing, 0));
            Vector2 v2 = editorView.WorldtoScreenSpace(new Vector2(0, 0));

            if ((v1 - v2).x < 5f)
            {
                if (m_gridVisible)
                    HideBackground();
            }
            else
            {
                if (!m_gridVisible)
                    ShowBackground();
            }
        }
        else
        {
            if (m_gridVisible)
                HideBackground();
        }
    }

}