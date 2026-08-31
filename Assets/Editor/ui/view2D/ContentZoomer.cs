// this code is an adapted version of 
// https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/GraphViewEditor/Manipulators/Zoomer.cs

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.View2D
{
    public class ContentZoomer : UnityEngine.UIElements.Manipulator
    {
        public static readonly float DefaultReferenceScale = 1;
        public static readonly float DefaultMinScale = 0.25f;
        public static readonly float DefaultMaxScale = 1;
        public static readonly float DefaultScaleStep = 0.15f;

        private Vector2 m_mousePosition = Vector2.zero;

        /// <summary>
        /// Scale that should be computed when scroll wheel offset is at zero.
        /// </summary>
        public float ReferenceScale { get; set; } = DefaultReferenceScale;

        public float MinScale { get; set; } = DefaultMinScale;
        public float MaxScale { get; set; } = DefaultMaxScale;

        /// <summary>
        /// Relative scale change when zooming in/out (e.g. For 15%, use 0.15).
        /// </summary>
        /// <remarks>
        /// Depending on the values of <c>minScale</c>, <c>maxScale</c> and <c>scaleStep</c>, it is not guaranteed that
        /// the first and last two scale steps will correspond exactly to the value specified in <c>scaleStep</c>.
        /// </remarks>
        public float ScaleStep { get; set; } = DefaultScaleStep;

        protected override void RegisterCallbacksOnTarget()
        {
            var gridView = target as GridView;
            if (gridView == null)
            {
                throw new InvalidOperationException("Manipulator can only be added to a GraphView");
            }

            target.RegisterCallback<WheelEvent>(OnWheel);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            EditorEventBus.Instance.ZoomChanged.Subscribe(OnZoomChanged);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<WheelEvent>(OnWheel);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            EditorEventBus.Instance.ZoomChanged.Unsubscribe(OnZoomChanged);
        }

        // Compute the parameters of our exponential model:
        // z(w) = (1 + s) ^ (w + a) + b
        // Where
        // z: calculated zoom level
        // w: accumulated wheel deltas (1 unit = 1 mouse notch)
        // s: zoom step
        //
        // The factors a and b are calculated in order to satisfy the conditions:
        // z(0) = referenceZoom
        // z(1) = referenceZoom * (1 + zoomStep)
        private static float CalculateNewZoom(float currentZoom, float wheelDelta, float zoomStep, float referenceZoom,
            float minZoom, float maxZoom)
        {
            if (minZoom <= 0)
            {
                Debug.LogError($"The minimum zoom ({minZoom}) must be greater than zero.");
                return currentZoom;
            }

            if (referenceZoom < minZoom)
            {
                Debug.LogError(
                    $"The reference zoom ({referenceZoom}) must be greater than or equal to the minimum zoom ({minZoom}).");
                return currentZoom;
            }

            if (referenceZoom > maxZoom)
            {
                Debug.LogError(
                    $"The reference zoom ({referenceZoom}) must be less than or equal to the maximum zoom ({maxZoom}).");
                return currentZoom;
            }

            if (zoomStep < 0)
            {
                Debug.LogError($"The zoom step ({zoomStep}) must be greater than or equal to zero.");
                return currentZoom;
            }

            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

            if (Mathf.Approximately(wheelDelta, 0))
            {
                return currentZoom;
            }

            // Calculate the factors of our model:
            double a = Math.Log(referenceZoom, 1 + zoomStep);
            double b = referenceZoom - Math.Pow(1 + zoomStep, a);

            // Convert zoom levels to scroll wheel values.
            double minWheel = Math.Log(minZoom - b, 1 + zoomStep) - a;
            double maxWheel = Math.Log(maxZoom - b, 1 + zoomStep) - a;
            double currentWheel = Math.Log(currentZoom - b, 1 + zoomStep) - a;

            // Except when the delta is zero, for each event, consider that the delta corresponds to a rotation by a
            // full notch. The scroll wheel abstraction system is buggy and incomplete: with a regular mouse, the
            // minimum wheel movement is 0.1 on OS X and 3 on Windows. We can't simply accumulate deltas like these, so
            // we accumulate integers only. This may be problematic with high resolution scroll wheels: many small
            // events will be fired. However, at this point, we have no way to differentiate a high resolution scroll
            // wheel delta from a non-accelerated scroll wheel delta of one notch on OS X.
            wheelDelta = Math.Sign(wheelDelta);
            currentWheel += wheelDelta;

            // Assimilate to the boundary when it is nearby.
            if (currentWheel > maxWheel - 0.5)
            {
                return maxZoom;
            }

            if (currentWheel < minWheel + 0.5)
            {
                return minZoom;
            }

            // Snap the wheel to the unit grid.
            currentWheel = Math.Round(currentWheel);

            // Do not assimilate again. Otherwise, points as far as 1.5 units away could be stuck to the boundary
            // because the wheel delta is either +1 or -1.

            // Calculate the corresponding zoom level.
            return (float)(Math.Pow(1 + zoomStep, currentWheel + a) + b);
        }

        private void OnWheel(WheelEvent evt)
        {
            IPanel panel = (evt.target as VisualElement)?.panel;
            if (panel.GetCapturingElement(PointerId.mousePointerId) != null)
                return;

            m_mousePosition = evt.localMousePosition;
            PrepareZoom(-evt.delta.y);
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            m_mousePosition = evt.localMousePosition;
        }

        private void OnZoomChanged(bool zoomedIn)
        {
            PrepareZoom(zoomedIn? 1.0f : -1.0f);
        }
        
        private void PrepareZoom(float delta)
        {
            var gridView = target as GridView;
            if (gridView == null)
                return;

            Vector3 position = gridView.contentViewContainer.resolvedStyle.translate;
            Vector3 scale = gridView.contentViewContainer.resolvedStyle.scale.value;

            Vector2 zoomCenter = new Vector2(
                (m_mousePosition.x - position.x) / scale.x,
                (m_mousePosition.y - position.y) / scale.y);
            float x = zoomCenter.x + gridView.contentViewContainer.layout.x;
            float y = zoomCenter.y + gridView.contentViewContainer.layout.y;
            position += Vector3.Scale(new Vector3(x, y, 0), scale);

            // Apply the new zoom.
            float zoom = CalculateNewZoom(scale.y, delta, ScaleStep, ReferenceScale, MinScale, MaxScale);
            scale.x = zoom;
            scale.y = zoom;
            scale.z = 1;

            position -= Vector3.Scale(new Vector3(x, y, 0), scale);

            gridView.UpdateViewTransform(position, scale);

        }

        /// <summary>
        /// Calculates the zoom level that fits a content-space rect (defined by <paramref name="min"/> and
        /// <paramref name="max"/>) into a viewport of the given size, respecting <paramref name="minScale"/> and
        /// <paramref name="maxScale"/>. Does not touch position or apply the transform — callers are responsible
        /// for that (typically via <c>GridView.UpdateViewTransform</c>, to stay consistent with the state
        /// <see cref="OnWheel"/> reads).
        /// </summary>
        /// <param name="min">Lower corner of the rect to fit, in content space.</param>
        /// <param name="max">Upper corner of the rect to fit, in content space.</param>
        /// <param name="viewportSize">Size of the viewport the rect must fit into.</param>
        /// <param name="minScale">Lower zoom bound to clamp to.</param>
        /// <param name="maxScale">Upper zoom bound to clamp to.</param>
        /// <param name="padding">
        /// Fraction of the fitted zoom to keep as margin (e.g. 0.9 leaves a 10% margin around the rect).
        /// Must be greater than zero. Defaults to 1 (no margin).
        /// </param>
        public static float CalculateZoomToFit(Vector2 min, Vector2 max, Vector2 viewportSize, float minScale,
            float maxScale, float padding = 1f)
        {
            if (padding <= 0)
            {
                Debug.LogError($"The padding ({padding}) must be greater than zero.");
                padding = 1f;
            }

            if (viewportSize.x <= 0 || viewportSize.y <= 0)
            {
                return Mathf.Clamp(1f, minScale, maxScale);
            }

            Vector2 rectSize = max - min;
            // Guard against degenerate rects (zero width/height, e.g. a single point or a line).
            bool validX = rectSize.x > Mathf.Epsilon;
            bool validY = rectSize.y > Mathf.Epsilon;

            if (!validX && !validY)
            {
                // Nothing to fit to: fall back to the reference scale.
                return Mathf.Clamp(DefaultReferenceScale, minScale, maxScale);
            }

            float zoomX = validX ? viewportSize.x / rectSize.x : float.PositiveInfinity;
            float zoomY = validY ? viewportSize.y / rectSize.y : float.PositiveInfinity;
            float zoom = Mathf.Min(zoomX, zoomY) * padding;
            return Mathf.Clamp(zoom, minScale, maxScale);
        }

        /// <summary>
        /// Applies a zoom (typically from <see cref="CalculateZoomToFit"/>) so that the given content-space rect
        /// ends up centered in the target's viewport. Goes through the same <c>UpdateViewTransform</c> path as
        /// <see cref="OnWheel"/>, so subsequent wheel zooming reads a consistent, up-to-date scale.
        /// </summary>
        /// <param name="min">Lower corner of the rect being framed, in content space.</param>
        /// <param name="max">Upper corner of the rect being framed, in content space.</param>
        /// <param name="zoom">
        /// The zoom level to apply, typically obtained via <see cref="CalculateZoomToFit"/>. If omitted, the
        /// current zoom is kept and only the position is adjusted to center the rect.
        /// </param>
        public void ApplyFrame(Vector2 min, Vector2 max, float? zoom = null)
        {
            if (target is not GridView gridView)
            {
                throw new InvalidOperationException("Manipulator can only be added to a GraphView");
            }

            float appliedZoom = zoom ?? gridView.contentViewContainer.resolvedStyle.scale.value.x;

            Vector2 viewportSize = gridView.layout.size;
            Vector2 rectCenter = (min + max) * 0.5f;
            Vector2 viewportCenter = viewportSize * 0.5f;

            // Same transform convention as OnWheel: screenPoint = contentPoint * scale + position
            Vector3 position = new Vector3(
                viewportCenter.x - rectCenter.x * appliedZoom,
                viewportCenter.y - rectCenter.y * appliedZoom,
                0);
            Vector3 scale = new Vector3(appliedZoom, appliedZoom, 1);

            gridView.UpdateViewTransform(position, scale);
        }

        /// <summary>
        /// Convenience combination of <see cref="CalculateZoomToFit"/> and <see cref="ApplyFrame"/>: computes the
        /// zoom that fits the given content-space rect into the viewport (respecting <see cref="MinScale"/> and
        /// <see cref="MaxScale"/>) and applies it, centering the rect.
        /// </summary>
        /// <param name="min">Lower corner of the rect to frame, in content space.</param>
        /// <param name="max">Upper corner of the rect to frame, in content space.</param>
        /// <param name="padding">
        /// Fraction of the fitted zoom to keep as margin (e.g. 0.9 leaves a 10% margin around the framed rect).
        /// Must be greater than zero. Defaults to 1 (no margin).
        /// </param>
        public void FrameToFit(Vector2 min, Vector2 max, float padding = 1f)
        {
            if (target is not GridView gridView)
            {
                throw new InvalidOperationException("Manipulator can only be added to a GraphView");
            }

            float zoom = CalculateZoomToFit(min, max, gridView.layout.size, MinScale, MaxScale, padding);
            ApplyFrame(min, max, zoom);
        }
    }
}
