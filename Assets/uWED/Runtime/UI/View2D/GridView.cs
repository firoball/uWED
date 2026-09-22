// this code is an adapted version of 
// https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/GraphViewEditor/Decorators/GridBackground.cs

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.View2D
{

    public abstract class GridView : VisualElement
    {

        private PropertyInfo m_roundToPixelGrid;

        public delegate void ViewTransformChanged(GridView gridView);

        public ViewTransformChanged viewTransformChanged { get; set; }


        private class ContentViewContainer : VisualElement
        {
            public override bool Overlaps(Rect r)
            {
                return true;
            }
        }

        private VisualElement gridViewContainer { get; }
        public VisualElement contentViewContainer { get; private set; }
        
        protected GridView()
        {
            AddToClassList("gridView");


            style.overflow = Overflow.Hidden;

            style.flexDirection = FlexDirection.Column;

            gridViewContainer = new VisualElement();
            gridViewContainer.style.flexGrow = 1f;
            gridViewContainer.style.flexBasis = 0f;
            gridViewContainer.pickingMode = PickingMode.Ignore;
            hierarchy.Add(gridViewContainer);

            contentViewContainer = new ContentViewContainer
            {
                name = "contentViewContainer",
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.GroupTransform
            };

            // make it absolute and 0 sized so it acts as a transform to move children to and from
            gridViewContainer.Add(contentViewContainer);

            focusable = true;
            m_roundToPixelGrid =
                typeof(GUIUtility).GetProperty("pixelsPerPoint", BindingFlags.NonPublic | BindingFlags.Static);
            if (m_roundToPixelGrid == null)
                Debug.LogError(
                    "Unable to bind 'GUIUtility.RoundToPixelGrid' - review whether Unity internals have changed");
        }

        private ContentZoomer m_zoomer;
        private float m_minScale = ContentZoomer.DefaultMinScale;
        private float m_maxScale = ContentZoomer.DefaultMaxScale;
        private float m_scaleStep = ContentZoomer.DefaultScaleStep;
        private float m_referenceScale = ContentZoomer.DefaultReferenceScale;

        protected ContentZoomer Zoomer => m_zoomer;
        public float MinScale => m_minScale;
        public float MaxScale => m_maxScale;
        public float ScaleStep => m_scaleStep;
        public float ReferenceScale => m_referenceScale;
        public float Scale => contentViewContainer.resolvedStyle.scale.value.x;

        public void SetupZoom(float minScaleSetup, float maxScaleSetup)
        {
            SetupZoom(minScaleSetup, maxScaleSetup, m_scaleStep, m_referenceScale);
        }

        public void SetupZoom(float minScaleSetup, float maxScaleSetup, float scaleStepSetup, float referenceScaleSetup)
        {
            m_minScale = minScaleSetup;
            m_maxScale = maxScaleSetup;
            m_scaleStep = scaleStepSetup;
            m_referenceScale = referenceScaleSetup;
            UpdateContentZoomer();
        }

        public void UpdateViewTransform(Vector3 newPosition, Vector3 newScale)
        {
            float validateFloat = newPosition.x + newPosition.y + newPosition.z + newScale.x + newScale.y + newScale.z;
            if (float.IsInfinity(validateFloat) || float.IsNaN(validateFloat))
                return;

            newPosition.x = RoundToPixelGrid(newPosition.x);
            newPosition.y = RoundToPixelGrid(newPosition.y);

            contentViewContainer.style.translate = newPosition;
            contentViewContainer.style.scale = newScale;

            if (viewTransformChanged != null)
                viewTransformChanged(this);
        }

        public float RoundToPixelGrid(float pos)
        {
            float ret;
            if (m_roundToPixelGrid != null)
            {
                float pixelsPerPoint = (float)m_roundToPixelGrid.GetValue(null);
                ret = Mathf.Round(pos * pixelsPerPoint) / pixelsPerPoint;
            }
            else
                ret = pos;

            return ret;
        }

        private void UpdateContentZoomer()
        {
            if (m_minScale != m_maxScale)
            {
                if (m_zoomer == null)
                {
                    m_zoomer = new ContentZoomer();
                    this.AddManipulator(m_zoomer);
                }

                m_zoomer.MinScale = m_minScale;
                m_zoomer.MaxScale = m_maxScale;
                m_zoomer.ScaleStep = m_scaleStep;
                m_zoomer.ReferenceScale = m_referenceScale;
            }
            else
            {
                if (m_zoomer != null)
                    this.RemoveManipulator(m_zoomer);
            }

            ValidateTransform();
        }

        protected void ValidateTransform()
        {
            if (contentViewContainer == null)
                return;
            
            Vector3 transformScale = contentViewContainer.resolvedStyle.scale.value;
            transformScale.x = Mathf.Clamp(transformScale.x, MinScale, MaxScale);
            transformScale.y = Mathf.Clamp(transformScale.y, MinScale, MaxScale);
            contentViewContainer.style.scale = transformScale;
        }

    }
}