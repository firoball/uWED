using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorInterface
{
    private EditorView m_ev;
    private GridManipulator m_gm;
    private EditorManipulator m_em;

    private readonly List<BaseField<bool>> m_toggleSnappingListeners = new List<BaseField<bool>>();
    private readonly List<PopupField<string>> m_setModeListeners = new List<PopupField<string>>();
    private readonly List<BaseField<int>> m_scaleGridListeners = new List<BaseField<int>>();
    private readonly List<BaseField<bool>> m_toggleGridListeners = new List<BaseField<bool>>();

    private bool m_toggleSnapping = false;
    private EditorStatus.Mode m_setMode = EditorStatus.Mode.Count;
    private float m_scaleGrid = 0f;
    private bool m_toggleGrid = false;

    public List<BaseField<bool>> ToggleSnappingListeners => m_toggleSnappingListeners;
    public List<PopupField<string>> SetModeListeners => m_setModeListeners;
    public List<BaseField<int>> ScaleGridListeners => m_scaleGridListeners;
    public List<BaseField<bool>> ToggleGridListeners => m_toggleGridListeners;


    public EditorInterface(EditorView ev, GridManipulator gm, EditorManipulator em)
    {
        m_ev = ev;
        m_gm = gm;
        m_em = em;
    }

    //Events from external components
    public void OnToggleSnapping(ChangeEvent<bool> evt)
    {
        m_ev?.ToggleSnapping(evt.newValue);
    }

    public void OnSetMode(ChangeEvent<string> evt) 
    {
        PopupField<string> field = evt.target as PopupField<string>;
        if (field != null && field.index >= 0 && field.index < (int)EditorStatus.Mode.Count)
        {
            m_em?.SetMode((EditorStatus.Mode)field.index);
        }
    }
    public void OnSetView(ChangeEvent<string> evt) { }
    public void OnScaleGrid(ChangeEvent<int> evt) 
    {
        m_gm?.ScaleGrid((float)evt.newValue);
    }
    public void OnToggleGrid(ChangeEvent<bool> evt) 
    {
        m_gm?.ToggleGrid(evt.newValue);
    }

    //Notifiers for listeners
    public void NotifyToggleSnappingListeners(bool value)
    {
        m_toggleSnapping = value;
        foreach (var listener in m_toggleSnappingListeners)
            listener.value = value;
    }

    public void NotifyScaleGridListeners(float value)
    {
        m_scaleGrid = value;
        foreach (var listener in m_scaleGridListeners)
            listener.value = (int)value;
    }

    public void NotifySetModeListeners(EditorStatus.Mode value)
    {
        m_setMode = value;
        foreach (var listener in m_setModeListeners)
            listener.index = (int)value;
    }

    public void NotifyToggleGridListeners(bool value)
    {
        m_toggleGrid = value;
        foreach (var listener in m_toggleGridListeners)
            listener.value = value;
    }

    //Notify all listeners
    public void RefreshListeners()
    {
        NotifyToggleSnappingListeners(m_toggleSnapping);
        NotifyScaleGridListeners(m_scaleGrid);
        NotifySetModeListeners(m_setMode);
        NotifyToggleGridListeners(m_toggleGrid);
    }
}