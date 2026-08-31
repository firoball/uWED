using Editor.UI.View2D;
using UnityEngine.UIElements;

/// <summary>
/// Translates UIElements ChangeEvent callbacks from the editor menu into
/// calls on the relevant editor components.
/// </summary>
public class EditorInterface
{
    private EditorView m_ev;
    private GridManipulator m_gm;
    private EditorManipulator m_em;

    public EditorInterface(EditorView ev, GridManipulator gm, EditorManipulator em)
    {
        m_ev = ev;
        m_gm = gm;
        m_em = em;
    }

    public void OnToggleSnapping(ChangeEvent<bool> evt)
    {
        m_ev?.ToggleSnapping(evt.newValue);
    }

    public void OnSetMode(ChangeEvent<string> evt)
    {
        if (evt.target is PopupField<string> field && field.index >= 0 && field.index < (int)EditorStatus.Mode.Count)
        {
            EditorStatus.Mode mode = (EditorStatus.Mode)field.index;
            m_em?.SetMode(mode);
        }
    }

    public void OnSetView(ChangeEvent<string> evt)
    {
    }

    public void OnScaleGrid(ChangeEvent<int> evt)
    {
        m_gm?.ScaleGrid((float)evt.newValue);
    }

    public void OnLockAngle(ChangeEvent<int> evt)
    {
        float angle = LockAngleUtility.IndexToDegrees(evt.newValue);
        m_ev?.LockAngle(angle);
    }

    public void OnToggleGrid(ChangeEvent<bool> evt)
    {
        m_gm?.ToggleGrid(evt.newValue);
    }

}
