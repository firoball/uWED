using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorView : GraphView
{
    const float c_pixelsPerUnit = 1f;
    const bool c_invertYPosition = true;

    private readonly GridManipulator m_gridManipulator;
    private readonly EditorManipulator m_editorManipulator;
    private readonly EditorInterface m_interface;
    private float m_lockAngle = 15.0f; //in deg
    private bool m_enableSnapping;

    public EditorInterface Interface => m_interface;

    public EditorView()
    {
        m_enableSnapping = true;
        FlexibleGridBackground grid = new FlexibleGridBackground();
        m_gridManipulator = new GridManipulator(grid);
        m_editorManipulator = new EditorManipulator();
        m_interface = new EditorInterface(this, m_gridManipulator, m_editorManipulator);
        name = "EditorView";
        this.StretchToParentSize();
        this.AddManipulator(m_gridManipulator); //must be added before Zoomer setup
        SetupZoom(ContentZoomer.DefaultMinScale * 0.1f, ContentZoomer.DefaultMaxScale * 4.0f);
        m_gridManipulator.RegisterCallbacksLate();//must be registered after Zoomer setup
        Add(grid);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(m_editorManipulator);

        //TODO: don't load transform prefs when map is new or has changed
        LoadPrefs();
        //pass defaults to all listeners
        m_interface.NotifyLockAngleListeners(m_lockAngle);
        m_interface.NotifyToggleSnappingListeners(m_enableSnapping);

        //this.generateVisualContent += GenerateVisualContent; //TODO: use this hook for drawing textured regions
        contentViewContainer.BringToFront();
        //TODO: only perform schedule.Execute when prefs were not found/loaded (e.g. new map)
        schedule.Execute(() =>
        {
            contentViewContainer.transform.position = parent.worldBound.size / 2f;
            //TODO: don't load transform prefs when map is new or has changed
            LoadPrefs();
        });
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.ClearItems(); //context menu SHUT UP!
    }
    /*void GenerateVisualContent(MeshGenerationContext m) //TODO: use this hook for drawing textured regions
    {
        Debug.Log("GenerateVisualContent");
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
        if (m_enableSnapping)
        {
            float degrees = angle * 180 / Mathf.PI; //angle in deg
            int snapped;
            if (degrees > 0.0f)
                snapped = (int)((degrees + 0.5f * m_lockAngle) / m_lockAngle);
            else
                snapped = (int)((degrees - 0.5f * m_lockAngle) / m_lockAngle);
            angle = snapped * m_lockAngle;
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

    public void LockAngle(float angle)
    {
        m_lockAngle = angle;
        m_interface.NotifyLockAngleListeners(m_lockAngle);
    }

    public void SavePrefs()
    {
        m_gridManipulator?.SavePrefs();
        m_editorManipulator?.SavePrefs();
        EditorPrefs.SetBool("uWED::EditorView::enableSnapping", m_enableSnapping);
        EditorPrefs.SetFloat("uWED::EditorView::lockAngle", m_lockAngle);
        EditorPrefs.SetFloat("uWED::EditorView::transform.position.x", contentViewContainer.transform.position.x);
        EditorPrefs.SetFloat("uWED::EditorView::transform.position.y", contentViewContainer.transform.position.y);
        EditorPrefs.SetFloat("uWED::EditorView::transform.scale.x", contentViewContainer.transform.scale.y);
        EditorPrefs.SetFloat("uWED::EditorView::transform.scale.y", contentViewContainer.transform.scale.y);
    }

    private void LoadPrefs()
    {
        if (EditorPrefs.HasKey("uWED::EditorView::enableSnapping"))
            m_enableSnapping = EditorPrefs.GetBool("uWED::EditorView::enableSnapping");
        if (EditorPrefs.HasKey("uWED::EditorView::lockAngle"))
            m_lockAngle = EditorPrefs.GetFloat("uWED::EditorView::lockAngle");

        //TODO: load pos and scale only if map has not changed
        Vector3 pos = contentViewContainer.transform.position;
        if (EditorPrefs.HasKey("uWED::EditorView::transform.position.x"))
            pos.x = EditorPrefs.GetFloat("uWED::EditorView::transform.position.x");
        if (EditorPrefs.HasKey("uWED::EditorView::transform.position.y"))
            pos.y = EditorPrefs.GetFloat("uWED::EditorView::transform.position.y");
        contentViewContainer.transform.position = pos;

        Vector3 scale = contentViewContainer.transform.scale;
        if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.x"))
            scale.x = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.x");
        if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.y"))
            scale.y = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.y");
        contentViewContainer.transform.scale = scale;
    }
}
