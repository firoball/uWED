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

namespace Editor.UI.View2D
{
    public class GridBackground : ImmediateModeElement
    {
        static readonly CustomStyleProperty<float> s_spacingProperty = new CustomStyleProperty<float>("--spacing");
        static readonly CustomStyleProperty<int> s_thickLinesProperty = new CustomStyleProperty<int>("--thick-lines");
        static readonly CustomStyleProperty<Color> s_lineColorProperty = new CustomStyleProperty<Color>("--line-color");

        static readonly CustomStyleProperty<Color> s_thickLineColorProperty =
            new CustomStyleProperty<Color>("--thick-line-color");

        static readonly CustomStyleProperty<Color> s_gridBackgroundColorProperty =
            new CustomStyleProperty<Color>("--grid-background-color");

        static readonly float s_defaultSpacing = 50f;
        static readonly int s_defaultThickLines = 10;
        static readonly Color s_defaultLineColor = new Color(0f, 0f, 0f, 0.18f);
        static readonly Color s_defaultThickLineColor = new Color(0f, 0f, 0f, 0.38f);
        static readonly Color s_defaultGridBackgroundColor = new Color(0.17f, 0.17f, 0.17f, 1.0f);

        private float m_spacing = s_defaultSpacing;
        private int m_thickLines = s_defaultThickLines;
        private Color m_lineColor = s_defaultLineColor;
        private Color m_thickLineColor = s_defaultThickLineColor;
        private Color m_gridBackgroundColor = s_defaultGridBackgroundColor;
        private VisualElement m_container;
        private bool m_enableDraw;

        //PATCH - start
        private readonly MethodInfo m_handleUtility;
        //PATCH - end

        public float Spacing
        {
            get => m_spacing;
            set => m_spacing = value;
        }
        
        public bool EnableDraw
        {
            get => m_enableDraw;
            set => m_enableDraw = value;
        }

        public GridBackground()
        {
            pickingMode = PickingMode.Ignore;

            this.StretchToParentSize();

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            //PATCH - start
            m_handleUtility = typeof(HandleUtility).GetMethod("ApplyWireMaterial",
                BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, Type.EmptyTypes, null);
            if (m_handleUtility == null)
                Debug.LogError(
                    "Unable to bind 'HandleUtility.ApplyWireMaterial' - review whether Unity internals have changed");
            //PATCH - end
            m_enableDraw = true;
        }

        private static Vector3 Clip(Rect clipRect, Vector3 inVec)
        {
            if (inVec.x < clipRect.xMin)
                inVec.x = clipRect.xMin;
            if (inVec.x > clipRect.xMax)
                inVec.x = clipRect.xMax;

            if (inVec.y < clipRect.yMin)
                inVec.y = clipRect.yMin;
            if (inVec.y > clipRect.yMax)
                inVec.y = clipRect.yMax;

            return inVec;
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
        {
            ICustomStyle customStyle = e.customStyle;
            if (customStyle.TryGetValue(s_spacingProperty, out var spacingValue))
                m_spacing = spacingValue;

            if (customStyle.TryGetValue(s_thickLinesProperty, out var thicklinesValue))
                m_thickLines = thicklinesValue;

            if (customStyle.TryGetValue(s_thickLineColorProperty, out var thicklineColorValue))
                m_thickLineColor = thicklineColorValue;

            if (customStyle.TryGetValue(s_lineColorProperty, out var lineColorValue))
                m_lineColor = lineColorValue;

            if (customStyle.TryGetValue(s_gridBackgroundColorProperty, out var gridColorValue))
                m_gridBackgroundColor = gridColorValue;
        }

        protected override void ImmediateRepaint()
        {
            VisualElement target = parent;

            var gridView = target as GridView;
            if (gridView == null)
            {
                throw new InvalidOperationException("GridBackground can only be added to a GridView");
            }

            // background
//PATCH - start
            //HandleUtility.ApplyWireMaterial();
            m_handleUtility?.Invoke(null, null);
//PATCH - end

            m_container = gridView.contentViewContainer;
            Rect clientRect = gridView.layout;

            // Since we're always stretch to parent size, we will use (0,0) as (x,y) coordinates
            clientRect.x = 0;
            clientRect.y = 0;
            
            DrawBackground(clientRect);
            if (m_enableDraw)
                DrawGrid(clientRect);
        }

        private void DrawBackground(Rect clientRect)
        {
            GL.Begin(GL.QUADS);
            GL.Color(m_gridBackgroundColor);
            GL.Vertex(new Vector3(clientRect.x, clientRect.y));
            GL.Vertex(new Vector3(clientRect.xMax, clientRect.y));
            GL.Vertex(new Vector3(clientRect.xMax, clientRect.yMax));
            GL.Vertex(new Vector3(clientRect.x, clientRect.yMax));
            GL.End();
        }
        
        private void DrawGrid(Rect clientRect)
        {
            var containerScale = m_container.resolvedStyle.scale.value;

            var t = m_container.resolvedStyle.translate;
            var containerTranslation = new Vector3(t.x, t.y, t.z);
            var containerPosition = m_container.layout;

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
            from.x = (from.x % (m_spacing * (containerScale.x)) - (m_spacing * (containerScale.x)));
            to.x = from.x;

            from.y = clientRect.y;
            to.y = clientRect.y + clientRect.height;

            while (from.x < clientRect.width)
            {
                from.x += m_spacing * containerScale.x;
                to.x += m_spacing * containerScale.x;

                GL.Begin(GL.LINES);
                GL.Color(m_lineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();
            }

            float thickLineSpacing = (m_spacing * m_thickLines);
            from.x = to.x = (thickGridLineX % (thickLineSpacing * (containerScale.x)) -
                             (thickLineSpacing * (containerScale.x)));

            while (from.x < clientRect.width + thickLineSpacing)
            {
                GL.Begin(GL.LINES);
                GL.Color(m_thickLineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();

                from.x += (m_spacing * containerScale.x * m_thickLines);
                to.x += (m_spacing * containerScale.x * m_thickLines);
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

            from.y = to.y = (from.y % (m_spacing * (containerScale.y)) - (m_spacing * (containerScale.y)));
            from.x = clientRect.x;
            to.x = clientRect.width;

            while (from.y < clientRect.height)
            {
                from.y += m_spacing * containerScale.y;
                to.y += m_spacing * containerScale.y;

                GL.Begin(GL.LINES);
                GL.Color(m_lineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();
            }

            thickLineSpacing = m_spacing * m_thickLines;
            from.y = to.y = (thickGridLineY % (thickLineSpacing * (containerScale.y)) -
                             (thickLineSpacing * (containerScale.y)));

            while (from.y < clientRect.height + thickLineSpacing)
            {
                GL.Begin(GL.LINES);
                GL.Color(m_thickLineColor);
                GL.Vertex(Clip(clientRect, from));
                GL.Vertex(Clip(clientRect, to));
                GL.End();

                from.y += m_spacing * containerScale.y * m_thickLines;
                to.y += m_spacing * containerScale.y * m_thickLines;
            }
        }
    }
}
