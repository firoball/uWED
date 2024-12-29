// this code is an adapted version of 
// https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/GraphViewEditor/Manipulators/ContentDragger.cs

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ContentDragger : MouseManipulator
{
    private Vector2 m_Start;
    public Vector2 panSpeed { get; set; }


    bool m_Active;

    public ContentDragger()
    {
        m_Active = false;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, modifiers = EventModifiers.Alt });
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.MiddleMouse });
        panSpeed = new Vector2(1, 1);
    }

    protected Rect CalculatePosition(float x, float y, float width, float height)
    {
        var rect = new Rect(x, y, width, height);
        return rect;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        var graphView = target as GridView;
        if (graphView == null)
        {
            throw new InvalidOperationException("Manipulator can only be added to a GraphView");
        }

        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected void OnMouseDown(MouseDownEvent e)
    {
        if (m_Active)
        {
            e.StopImmediatePropagation();
            return;
        }

        if (!CanStartManipulation(e))
            return;

        var gridView = target as GridView;
        if (gridView == null)
            return;

        m_Start = gridView.ChangeCoordinatesTo(gridView.contentViewContainer, e.localMousePosition);

        m_Active = true;
        target.CaptureMouse();
        e.StopImmediatePropagation();
    }

    protected void OnMouseMove(MouseMoveEvent e)
    {
        if (!m_Active)
            return;

        var gridView = target as GridView;
        if (gridView == null)
            return;

        Vector2 diff = gridView.ChangeCoordinatesTo(gridView.contentViewContainer, e.localMousePosition) - m_Start;

        // During the drag update only the view
        Vector3 s = gridView.contentViewContainer.transform.scale;
        gridView.viewTransform.position += Vector3.Scale(diff, s);

        e.StopPropagation();
    }

    protected void OnMouseUp(MouseUpEvent e)
    {
        if (!m_Active || !CanStopManipulation(e))
            return;

        var gridView = target as GridView;
        if (gridView == null)
            return;

        Vector3 p = gridView.contentViewContainer.transform.position;
        Vector3 s = gridView.contentViewContainer.transform.scale;

        gridView.UpdateViewTransform(p, s);

        m_Active = false;
        target.ReleaseMouse();
        e.StopPropagation();
    }
}
