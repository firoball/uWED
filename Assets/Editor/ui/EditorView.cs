using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorView : GraphView
{
    const float c_pixelsPerUnit = 1f;
    const bool c_invertYPosition = true;
    const float c_angleSnapping = 15.0f; //TODO: temp - hook to UI (in deg)

    private readonly GridManipulator m_gridManipulator;
    private readonly EditorManipulator m_editorManipulator;
    private readonly EditorInterface m_interface;
    private bool m_enableSnapping;
    private bool m_enableAngleSnapping;

    public EditorInterface Interface => m_interface;

    public EditorView()
    {
        m_enableSnapping = true;
        m_enableAngleSnapping = true;
        FlexibleGridBackground grid = new FlexibleGridBackground();
        m_gridManipulator = new GridManipulator(grid);
        m_editorManipulator = new EditorManipulator();
        m_interface = new EditorInterface(this, m_gridManipulator, m_editorManipulator);
        name = "EditorView";
        this.StretchToParentSize();
        this.AddManipulator(m_gridManipulator); //must be added before Zoomer setup
        SetupZoom(ContentZoomer.DefaultMinScale * 0.5f, ContentZoomer.DefaultMaxScale * 3.0f);
        m_gridManipulator.RegisterCallbacksLate();//must be registered after Zoomer setup
        Add(grid);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(m_editorManipulator);
        //pass defaults to all listeners
        m_interface.NotifyToggleSnappingListeners(m_enableSnapping);

        //this.generateVisualContent += Test; //TODO: use this hook for drawing textured regions
        contentViewContainer.BringToFront();
        //contentViewContainer.Add(new Label { name = "origin", text = "(0,0)" });
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
        var position = pos * c_pixelsPerUnit - contentViewContainer.layout.position;
        if (c_invertYPosition) position.y = -position.y;
        return contentViewContainer.transform.matrix.MultiplyPoint3x4(position);
    }

    public Vector2 ScreenToWorldSpace(Vector2 pos)
    {
        Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint3x4(pos);
        if (c_invertYPosition) position.y = -position.y;
        return (position + contentViewContainer.layout.position) / c_pixelsPerUnit;
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

    public float SnapAngle(float angle) //angle in rad
    {
        if (m_enableAngleSnapping)
        {
            float degrees = angle * 180 / Mathf.PI; //angle in deg
            int snapped;
            if (degrees > 0.0f)
                snapped = (int)((degrees + 0.5f * c_angleSnapping) / c_angleSnapping);
            else
                snapped = (int)((degrees - 0.5f * c_angleSnapping) / c_angleSnapping);
            angle = snapped * c_angleSnapping;
            if (angle <= -180f) angle = 180f; //edge case: -180 deg -> 180 deg
            return (angle / 180) * Mathf.PI; //angle in rad
        }
        else
        {
            return angle; //angle in rad
        }
    }

    public void ToggleSnapping(bool enable)
    {
        m_enableSnapping = enable;
        m_interface.NotifyToggleSnappingListeners(m_enableSnapping);
    }

    public void ToggleAngleSnapping(bool enable)
    {
        m_enableAngleSnapping = enable;
        //m_interface.NotifyToggleAngleSnappingListeners(m_enableAngleSnapping);
    }

}
