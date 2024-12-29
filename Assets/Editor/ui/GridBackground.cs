// this code is an adapted version of 
// https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/GraphViewEditor/Decorators/GridBackground.cs

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
//PATCH - start
using System.Reflection;
using UnityEditor;
//PATCH - end
using UnityEngine;
using UnityEngine.UIElements;

public class GridBackground : ImmediateModeElement
{
    static CustomStyleProperty<float> s_SpacingProperty = new CustomStyleProperty<float>("--spacing");
    static CustomStyleProperty<int> s_ThickLinesProperty = new CustomStyleProperty<int>("--thick-lines");
    static CustomStyleProperty<Color> s_LineColorProperty = new CustomStyleProperty<Color>("--line-color");
    static CustomStyleProperty<Color> s_ThickLineColorProperty = new CustomStyleProperty<Color>("--thick-line-color");
    static CustomStyleProperty<Color> s_GridBackgroundColorProperty = new CustomStyleProperty<Color>("--grid-background-color");

    static readonly float s_DefaultSpacing = 50f;
    static readonly int s_DefaultThickLines = 10;
    static readonly Color s_DefaultLineColor = new Color(0f, 0f, 0f, 0.18f);
    static readonly Color s_DefaultThickLineColor = new Color(0f, 0f, 0f, 0.38f);
    static readonly Color s_DefaultGridBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1.0f);

    float m_Spacing = s_DefaultSpacing;
    private float spacing => m_Spacing;

    int m_ThickLines = s_DefaultThickLines;
    private int thickLines => m_ThickLines;

    Color m_LineColor = s_DefaultLineColor;
    private Color lineColor => m_LineColor * Color.white;// UIElementsUtility.editorPlayModeTintColor;

    Color m_ThickLineColor = s_DefaultThickLineColor;
    private Color thickLineColor => m_ThickLineColor * Color.white;// UIElementsUtility.editorPlayModeTintColor;

    Color m_GridBackgroundColor = s_DefaultGridBackgroundColor;
    private Color gridBackgroundColor => m_GridBackgroundColor * Color.white;// UIElementsUtility.editorPlayModeTintColor;

    private VisualElement m_Container;

//PATCH - start
    private MethodInfo m_handleUtility;

    public float Spacing { get => m_Spacing; set => m_Spacing = value; }
    public int ThickLines { get => m_ThickLines; set => m_ThickLines = value; }
    public Color LineColor { get => m_LineColor; set => m_LineColor = value; }
    public Color ThickLineColor { get => m_ThickLineColor; set => m_ThickLineColor = value; }
    public Color GridBackgroundColor { get => m_GridBackgroundColor; set => m_GridBackgroundColor = value; }
//PATCH - end

    public GridBackground()
    {
        pickingMode = PickingMode.Ignore;

        this.StretchToParentSize();

        RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        //PATCH - start
        m_handleUtility = typeof(HandleUtility).GetMethod("ApplyWireMaterial", BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, Type.EmptyTypes, null);
        if (m_handleUtility == null) Debug.LogError("Unable to bind 'HandleUtility.ApplyWireMaterial' - review whether Unity internals have changed");
        //PATCH - end
    }

private Vector3 Clip(Rect clipRect, Vector3 _in)
    {
        if (_in.x < clipRect.xMin)
            _in.x = clipRect.xMin;
        if (_in.x > clipRect.xMax)
            _in.x = clipRect.xMax;

        if (_in.y < clipRect.yMin)
            _in.y = clipRect.yMin;
        if (_in.y > clipRect.yMax)
            _in.y = clipRect.yMax;

        return _in;
    }

    private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
    {
        float spacingValue = 0f;
        int thicklinesValue = 0;
        Color thicklineColorValue = Color.clear;
        Color lineColorValue = Color.clear;
        Color gridColorValue = Color.clear;

        ICustomStyle customStyle = e.customStyle;
        if (customStyle.TryGetValue(s_SpacingProperty, out spacingValue))
            m_Spacing = spacingValue;

        if (customStyle.TryGetValue(s_ThickLinesProperty, out thicklinesValue))
            m_ThickLines = thickLines;

        if (customStyle.TryGetValue(s_ThickLineColorProperty, out thicklineColorValue))
            m_ThickLineColor = thicklineColorValue;

        if (customStyle.TryGetValue(s_LineColorProperty, out lineColorValue))
            m_LineColor = lineColorValue;

        if (customStyle.TryGetValue(s_GridBackgroundColorProperty, out gridColorValue))
            m_GridBackgroundColor = gridColorValue;
    }

    protected override void ImmediateRepaint()
    {
        VisualElement target = parent;

        var gridView = target as GridView;
        if (gridView == null)
        {
            throw new InvalidOperationException("GridBackground can only be added to a GridView");
        }
        m_Container = gridView.contentViewContainer;
        Rect clientRect = gridView.layout;

        // Since we're always stretch to parent size, we will use (0,0) as (x,y) coordinates
        clientRect.x = 0;
        clientRect.y = 0;

        var containerScale = new Vector3(m_Container.transform.matrix.GetColumn(0).magnitude,
            m_Container.transform.matrix.GetColumn(1).magnitude,
            m_Container.transform.matrix.GetColumn(2).magnitude);
        var containerTranslation = m_Container.transform.matrix.GetColumn(3);
        var containerPosition = m_Container.layout;

    // background
//PATCH - start
        //HandleUtility.ApplyWireMaterial();
        m_handleUtility?.Invoke(null, null);
//PATCH - end

        GL.Begin(GL.QUADS);
        GL.Color(gridBackgroundColor);
        GL.Vertex(new Vector3(clientRect.x, clientRect.y));
        GL.Vertex(new Vector3(clientRect.xMax, clientRect.y));
        GL.Vertex(new Vector3(clientRect.xMax, clientRect.yMax));
        GL.Vertex(new Vector3(clientRect.x, clientRect.yMax));
        GL.End();

        // vertical lines
        Vector3 from = new Vector3(clientRect.x, clientRect.y, 0.0f);
        Vector3 to = new Vector3(clientRect.x, clientRect.height, 0.0f);

        var tx = Matrix4x4.TRS(containerTranslation, Quaternion.identity, Vector3.one);

        from = tx.MultiplyPoint(from);
        to = tx.MultiplyPoint(to);

        from.x += (containerPosition.x * containerScale.x);
        from.y += (containerPosition.y * containerScale.y);
        to.x += (containerPosition.x * containerScale.x);
        to.y += (containerPosition.y * containerScale.y);

        float thickGridLineX = from.x;
        float thickGridLineY = from.y;

        // Update from/to to start at beginning of clientRect
        from.x = (from.x % (spacing * (containerScale.x)) - (spacing * (containerScale.x)));
        to.x = from.x;

        from.y = clientRect.y;
        to.y = clientRect.y + clientRect.height;

        while (from.x < clientRect.width)
        {
            from.x += spacing * containerScale.x;
            to.x += spacing * containerScale.x;

            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex(Clip(clientRect, from));
            GL.Vertex(Clip(clientRect, to));
            GL.End();
        }

        float thickLineSpacing = (spacing * thickLines);
        from.x = to.x = (thickGridLineX % (thickLineSpacing * (containerScale.x)) - (thickLineSpacing * (containerScale.x)));

        while (from.x < clientRect.width + thickLineSpacing)
        {
            GL.Begin(GL.LINES);
            GL.Color(thickLineColor);
            GL.Vertex(Clip(clientRect, from));
            GL.Vertex(Clip(clientRect, to));
            GL.End();

            from.x += (spacing * containerScale.x * thickLines);
            to.x += (spacing * containerScale.x * thickLines);
        }

        // horizontal lines
        from = new Vector3(clientRect.x, clientRect.y, 0.0f);
        to = new Vector3(clientRect.x + clientRect.width, clientRect.y, 0.0f);

        from.x += (containerPosition.x * containerScale.x);
        from.y += (containerPosition.y * containerScale.y);
        to.x += (containerPosition.x * containerScale.x);
        to.y += (containerPosition.y * containerScale.y);

        from = tx.MultiplyPoint(from);
        to = tx.MultiplyPoint(to);

        from.y = to.y = (from.y % (spacing * (containerScale.y)) - (spacing * (containerScale.y)));
        from.x = clientRect.x;
        to.x = clientRect.width;

        while (from.y < clientRect.height)
        {
            from.y += spacing * containerScale.y;
            to.y += spacing * containerScale.y;

            GL.Begin(GL.LINES);
            GL.Color(lineColor);
            GL.Vertex(Clip(clientRect, from));
            GL.Vertex(Clip(clientRect, to));
            GL.End();
        }

        thickLineSpacing = spacing * thickLines;
        from.y = to.y = (thickGridLineY % (thickLineSpacing * (containerScale.y)) - (thickLineSpacing * (containerScale.y)));

        while (from.y < clientRect.height + thickLineSpacing)
        {
            GL.Begin(GL.LINES);
            GL.Color(thickLineColor);
            GL.Vertex(Clip(clientRect, from));
            GL.Vertex(Clip(clientRect, to));
            GL.End();

            from.y += spacing * containerScale.y * thickLines;
            to.y += spacing * containerScale.y * thickLines;
        }
    }
}
