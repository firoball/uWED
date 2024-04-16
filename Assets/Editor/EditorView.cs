using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorView : GraphView
{
    const float m_pixelsPerUnit = 1f;
    const bool m_invertYPosition = true;

    private readonly GridManipulator m_gridManipulator;
    private readonly EditorManipulator m_editorManipulator;
    private bool m_enableSnapping;

    public GridManipulator GridManipulator => m_gridManipulator;
    public EditorManipulator EditorManipulator => m_editorManipulator;

    public EditorView()
    {
        m_enableSnapping = true;
        FlexibleGridBackground grid = new FlexibleGridBackground();
        m_gridManipulator = new GridManipulator(grid);
        m_editorManipulator = new EditorManipulator();
        name = "EditorView";
        this.StretchToParentSize();
        //RegisterCallback<WheelEvent>(OnMouseWheel); //must be registered prior to Zoomer setup
        this.AddManipulator(m_gridManipulator);
        SetupZoom(ContentZoomer.DefaultMinScale * 0.5f, ContentZoomer.DefaultMaxScale * 3.0f);
        RegisterCallback<WheelEvent>(m_gridManipulator.OnWheelLate); //must be registered after Zoomer setup
        Add(grid);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(m_editorManipulator);
        //other things that might interest you
        //this.AddManipulator(new SelectionDragger());
        //this.AddManipulator(new RectangleSelector());
        //this.AddManipulator(new ClickSelector());
        //this.generateVisualContent += Test; //TODO: use this hook for drawing textured regions
        contentViewContainer.Add(new Label { name = "origin", text = "(0,0)" });
        contentViewContainer.BringToFront();
        schedule.Execute(() =>
        {
            contentViewContainer.transform.position = parent.worldBound.size / 2f;
        });
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.ClearItems(); //context menu SHUT UP!
    }
    /*void Test(MeshGenerationContext m) //TODO: use this hook for drawing textured regions
    {
        Debug.Log("GVC Test");
    }*/

    public Vector2 WorldtoScreenSpace(Vector2 pos)
    {
        var position = pos * m_pixelsPerUnit - contentViewContainer.layout.position;
        if (m_invertYPosition) position.y = -position.y;
        return contentViewContainer.transform.matrix.MultiplyPoint3x4(position);
    }

    public Vector2 ScreenToWorldSpace(Vector2 pos)
    {
        Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint3x4(pos);
        if (m_invertYPosition) position.y = -position.y;
        return (position + contentViewContainer.layout.position) / m_pixelsPerUnit;
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
            return WorldtoScreenSpace(snappedPos);
        }
        else
        {
            return pos;
        }
    }

    public void OnToggleSnapping(ChangeEvent<bool> evt)
    {
        m_enableSnapping = evt.newValue;
    }
}
