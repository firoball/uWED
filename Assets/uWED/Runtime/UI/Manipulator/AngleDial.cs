using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace uWED.Runtime.UI.Manipulator
{
    /// <summary>
    /// Circular angle selector in degrees: 0 points right and 90 up, the angle grows counter-clockwise.
    /// Ticks mark every 45 degrees, the four main directions (0, 90, 180, 270) longer. Pressing or dragging
    /// sets the angle to the nearest multiple of Step. The value is wrapped to 0..360.
    /// </summary>
    public class AngleDial : VisualElement
    {
        const float DialSize = 64f;
        const float RingRadius = 28f;
        const float MainTickLength = 8f;
        const float MinorTickLength = 5f;
        const float KnobRadius = 3.5f;
        const float TickAngle = 45f;

        static readonly Color s_fillColor = new Color(0.15f, 0.15f, 0.15f);
        static readonly Color s_ringColor = new Color(0.43f, 0.43f, 0.43f);
        static readonly Color s_minorTickColor = new Color(0.59f, 0.59f, 0.59f);
        static readonly Color s_mainTickColor = new Color(0.86f, 0.86f, 0.86f);
        static readonly Color s_pointerColor = new Color(1f, 0.8f, 0.1f);

        float m_value;
        float m_step = 15f;

        /// <summary>Raised with the new angle in degrees when the user changes it. Not raised by setting Value.</summary>
        public event Action<float> ValueChanged;

        /// <summary>Current angle in degrees, wrapped to 0..360.</summary>
        public float Value
        {
            get => m_value;
            set
            {
                m_value = Mathf.Repeat(value, 360f);
                MarkDirtyRepaint();
            }
        }

        /// <summary>Angle the user's input snaps to, in degrees. Zero or less leaves the input unsnapped.</summary>
        public float Step
        {
            get => m_step;
            set => m_step = value;
        }

        /// <summary>Creates a dial at angle 0.</summary>
        public AngleDial()
        {
            style.width = DialSize;
            style.height = DialSize;
            style.flexShrink = 0;
            generateVisualContent += OnGenerateVisualContent;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            Value = 0f;
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            this.CapturePointer(evt.pointerId);
            SetFromPosition(evt.localPosition);
            evt.StopPropagation();
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                SetFromPosition(evt.localPosition);
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
        }

        void SetFromPosition(Vector2 localPosition)
        {
            Vector2 center = contentRect.center;
            float degrees = Mathf.Atan2(center.y - localPosition.y, localPosition.x - center.x) * Mathf.Rad2Deg;
            if (m_step > 0f)
                degrees = Mathf.Round(degrees / m_step) * m_step;

            float previous = m_value;
            Value = degrees;
            if (!Mathf.Approximately(previous, m_value))
                ValueChanged?.Invoke(m_value);
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            Vector2 center = contentRect.center;

            painter.fillColor = s_fillColor;
            painter.BeginPath();
            painter.Arc(center, RingRadius, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();

            painter.lineWidth = 1f;
            painter.strokeColor = s_ringColor;
            painter.BeginPath();
            painter.Arc(center, RingRadius, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Stroke();

            for (float tick = 0f; tick < 360f; tick += TickAngle)
            {
                bool main = Mathf.Approximately(tick % 90f, 0f);
                painter.lineWidth = main ? 2f : 1f;
                painter.strokeColor = main ? s_mainTickColor : s_minorTickColor;
                painter.BeginPath();
                painter.MoveTo(PointOnRing(center, tick, RingRadius));
                painter.LineTo(PointOnRing(center, tick, RingRadius - (main ? MainTickLength : MinorTickLength)));
                painter.Stroke();
            }

            Vector2 tip = PointOnRing(center, m_value, RingRadius - MainTickLength - 2f);
            painter.lineWidth = 2f;
            painter.strokeColor = s_pointerColor;
            painter.BeginPath();
            painter.MoveTo(center);
            painter.LineTo(tip);
            painter.Stroke();

            painter.fillColor = s_pointerColor;
            painter.BeginPath();
            painter.Arc(tip, KnobRadius, Angle.Degrees(0f), Angle.Degrees(360f));
            painter.Fill();
        }

        /// <summary>Point at the given angle (0 right, 90 up) and distance from center, in element coordinates (y down).</summary>
        static Vector2 PointOnRing(Vector2 center, float degrees, float radius)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(center.x + Mathf.Cos(radians) * radius, center.y - Mathf.Sin(radians) * radius);
        }
    }
}
