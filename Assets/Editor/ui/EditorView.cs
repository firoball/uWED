using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorView : GridView
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
        GridBackground grid = new GridBackground();
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
#pragma warning disable CS0618 // Type or member is obsolete
            contentViewContainer.transform.position = parent.worldBound.size / 2f;
#pragma warning restore CS0618 // Type or member is obsolete
            //TODO: don't load transform prefs when map is new or has changed
            LoadPrefs();
        });
    }

    /*void GenerateVisualContent(MeshGenerationContext m) //TODO: use this hook for drawing textured regions
    {
        Debug.Log("GenerateVisualContent");
    }*/

    public Matrix4x4 WorldToScreenMatrix()
    {
        //layout offset - this is already in screen coordinates
        Vector3 layoutTranslate = new Vector3(contentViewContainer.layout.position.x, contentViewContainer.layout.position.y);
        //TRS of content container
#pragma warning disable CS0618 // Type or member is obsolete
        Vector3 translate = contentViewContainer.transform.position;
        Quaternion rotate = contentViewContainer.transform.rotation;
        Vector3 scale = contentViewContainer.transform.scale;
#pragma warning restore CS0618 // Type or member is obsolete
        //invert y if configured
        if (c_invertYPosition)
        {
            layoutTranslate.y *= -1;
            scale.y *= -1;
        }
        //configured pixel resolution
        Vector3 pixelScale = new Vector3(c_pixelsPerUnit, c_pixelsPerUnit);
        //build actual world to screen matrix
        return Matrix4x4.TRS(translate, rotate, scale) * Matrix4x4.Translate(layoutTranslate) * Matrix4x4.Scale(pixelScale);
    }

    public Vector2 WorldtoScreenSpace(Vector2 pos)
    {
        var position = pos * c_pixelsPerUnit - contentViewContainer.layout.position;
        if (c_invertYPosition) position.y = -position.y;
#pragma warning disable CS0618 // Type or member is obsolete
        return contentViewContainer.transform.matrix.MultiplyPoint3x4(position);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public Vector2 ScreenToWorldSpace(Vector2 pos)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Vector2 position = contentViewContainer.transform.matrix.inverse.MultiplyPoint3x4(pos);
#pragma warning restore CS0618 // Type or member is obsolete
        if (c_invertYPosition) position.y = -position.y;
        return (position + contentViewContainer.layout.position) / c_pixelsPerUnit;
    }

    public float ScaleScreenToWorld(float length)
    {
        //Debug.Log(length + " " + contentViewContainer.transform.scale.x + " " + length / contentViewContainer.transform.scale.x);
#pragma warning disable CS0618 // Type or member is obsolete
        return length / contentViewContainer.transform.scale.x / c_pixelsPerUnit;
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
        EditorPrefs.SetFloat("uWED::EditorView::transform.position.x", contentViewContainer.transform.position.x);
        EditorPrefs.SetFloat("uWED::EditorView::transform.position.y", contentViewContainer.transform.position.y);
        EditorPrefs.SetFloat("uWED::EditorView::transform.scale.x", contentViewContainer.transform.scale.y);
        EditorPrefs.SetFloat("uWED::EditorView::transform.scale.y", contentViewContainer.transform.scale.y);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private void LoadPrefs()
    {
        if (EditorPrefs.HasKey("uWED::EditorView::enableSnapping"))
            m_enableSnapping = EditorPrefs.GetBool("uWED::EditorView::enableSnapping");
        if (EditorPrefs.HasKey("uWED::EditorView::lockAngle"))
            m_lockAngle = EditorPrefs.GetFloat("uWED::EditorView::lockAngle");

        //TODO: load pos and scale only if map has not changed
#pragma warning disable CS0618 // Type or member is obsolete
        Vector3 pos = contentViewContainer.transform.position;
#pragma warning restore CS0618 // Type or member is obsolete
        if (EditorPrefs.HasKey("uWED::EditorView::transform.position.x"))
            pos.x = EditorPrefs.GetFloat("uWED::EditorView::transform.position.x");
        if (EditorPrefs.HasKey("uWED::EditorView::transform.position.y"))
            pos.y = EditorPrefs.GetFloat("uWED::EditorView::transform.position.y");
#pragma warning disable CS0618 // Type or member is obsolete
        contentViewContainer.transform.position = pos;
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
        Vector3 scale = contentViewContainer.transform.scale;
#pragma warning restore CS0618 // Type or member is obsolete
        if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.x"))
            scale.x = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.x");
        if (EditorPrefs.HasKey("uWED::EditorView::transform.scale.y"))
            scale.y = EditorPrefs.GetFloat("uWED::EditorView::transform.scale.y");
#pragma warning disable CS0618 // Type or member is obsolete
        contentViewContainer.transform.scale = scale;
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
