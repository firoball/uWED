// this code is an adapted version of 
// https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Modules/GraphViewEditor/Decorators/GridBackground.cs

// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;


public abstract class GridView : VisualElement
{

    private MethodInfo m_roundToPixelGrid;

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


    public ITransform viewTransform
    {
        get { return contentViewContainer.transform; }
    }

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
        m_roundToPixelGrid = typeof(GUIUtility).GetMethod("RoundToPixelGrid", BindingFlags.NonPublic | BindingFlags.Static, Type.DefaultBinder, new Type[] { typeof(float) }, null);
        if (m_roundToPixelGrid == null) Debug.LogError("Unable to bind 'GUIUtility.RoundToPixelGrid' - review whether Unity internals have changed");
    }

    private ContentZoomer m_Zoomer;
    private float m_MinScale = ContentZoomer.DefaultMinScale;
    private float m_MaxScale = ContentZoomer.DefaultMaxScale;
    private float m_ScaleStep = ContentZoomer.DefaultScaleStep;
    private float m_ReferenceScale = ContentZoomer.DefaultReferenceScale;

    public float minScale
    {
        get { return m_MinScale; }
    }

    public float maxScale
    {
        get { return m_MaxScale; }
    }

    public float scaleStep
    {
        get { return m_ScaleStep; }
    }

    public float referenceScale
    {
        get { return m_ReferenceScale; }
    }

    public float scale
    {
        get { return viewTransform.scale.x; }
    }

    public void SetupZoom(float minScaleSetup, float maxScaleSetup)
    {
        SetupZoom(minScaleSetup, maxScaleSetup, m_ScaleStep, m_ReferenceScale);
    }

    public void SetupZoom(float minScaleSetup, float maxScaleSetup, float scaleStepSetup, float referenceScaleSetup)
    {
        m_MinScale = minScaleSetup;
        m_MaxScale = maxScaleSetup;
        m_ScaleStep = scaleStepSetup;
        m_ReferenceScale = referenceScaleSetup;
        UpdateContentZoomer();
    }

    public void UpdateViewTransform(Vector3 newPosition, Vector3 newScale)
    {
        float validateFloat = newPosition.x + newPosition.y + newPosition.z + newScale.x + newScale.y + newScale.z;
        if (float.IsInfinity(validateFloat) || float.IsNaN(validateFloat))
            return;

        //newPosition.x = GUIUtility.RoundToPixelGrid(newPosition.x);
        //newPosition.y = GUIUtility.RoundToPixelGrid(newPosition.y);
        newPosition.x = RoundToPixelGrid(newPosition.x);
        newPosition.y = RoundToPixelGrid(newPosition.y);

        contentViewContainer.transform.position = newPosition;
        contentViewContainer.transform.scale = newScale;

        if (viewTransformChanged != null)
            viewTransformChanged(this);
    }

    public float RoundToPixelGrid(float pos)
    {
        float ret;
        if (m_roundToPixelGrid != null)
            ret = (float)m_roundToPixelGrid.Invoke(null, new[] { (object)pos });
        else
            ret = pos;

        return ret;
    }

    private void UpdateContentZoomer()
    {
        if (m_MinScale != m_MaxScale)
        {
            if (m_Zoomer == null)
            {
                m_Zoomer = new ContentZoomer();
                this.AddManipulator(m_Zoomer);
            }

            m_Zoomer.minScale = m_MinScale;
            m_Zoomer.maxScale = m_MaxScale;
            m_Zoomer.scaleStep = m_ScaleStep;
            m_Zoomer.referenceScale = m_ReferenceScale;
        }
        else
        {
            if (m_Zoomer != null)
                this.RemoveManipulator(m_Zoomer);
        }

        ValidateTransform();
    }

    protected void ValidateTransform()
    {
        if (contentViewContainer == null)
            return;
        Vector3 transformScale = viewTransform.scale;

        transformScale.x = Mathf.Clamp(transformScale.x, minScale, maxScale);
        transformScale.y = Mathf.Clamp(transformScale.y, minScale, maxScale);

        viewTransform.scale = transformScale;
    }

}
