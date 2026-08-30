using System;
using System.Collections.Generic;
using Editor.UI.View2D;
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
    private readonly List<BaseField<int>> m_lockAngleListeners = new List<BaseField<int>>();
    private readonly List<BaseField<bool>> m_toggleGridListeners = new List<BaseField<bool>>();

    #region buffers

    private bool m_toggleSnapping = false;
    private EditorStatus.Mode m_setMode = EditorStatus.Mode.Count;
    private EditorStatus.Construct m_setConstructionMode = EditorStatus.Construct.Idle;
    private float m_scaleGrid = 0f;
    private float m_lockAngle = 0f;
    private bool m_toggleGrid = false;

    #endregion

    #region event hooks

    public event Action<Vector2> OnMouseMoved;
    public event Action<CursorInfo> OnCursorInfoChanged;
    public event Action<Mesh> OnRegionMeshChanged;
    public event Action<EditorStatus.Mode> OnModeChanged;
    public event Action<EditorStatus.Construct> OnConstructionModeChanged;
    public event Action<MapObject, List<string>> OnEditObject;
    public event Action<Vertex> OnEditVertex;
    public event Action<Segment, List<string>> OnEditSegment;
    public event Action<Region, List<string>> OnEditRegion;
    public event Action<Way, List<string>> OnEditWay;

    public List<BaseField<bool>> ToggleSnappingListeners => m_toggleSnappingListeners;
    public List<PopupField<string>> SetModeListeners => m_setModeListeners;
    public List<BaseField<int>> ScaleGridListeners => m_scaleGridListeners;
    public List<BaseField<int>> LockAngleListeners => m_lockAngleListeners;
    public List<BaseField<bool>> ToggleGridListeners => m_toggleGridListeners;

    #endregion


    public EditorInterface(EditorView ev, GridManipulator gm, EditorManipulator em)
    {
        m_ev = ev;
        m_gm = gm;
        m_em = em;
    }

    #region incoming events

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

    public void OnSetView(ChangeEvent<string> evt)
    {
    }

    public void OnScaleGrid(ChangeEvent<int> evt)
    {
        m_gm?.ScaleGrid((float)evt.newValue);
    }

    public void OnLockAngle(ChangeEvent<int> evt)
    {
        float angle;
        int value = evt.newValue;
        if (value == 1 || value == 2) //1, 2
            angle = (float)value;
        else if (value == 3 || value == 4) //5, 10
            angle = (float)((value - 2) * 5);
        else if (value == 11) //120
            angle = 120f;
        else //15, 30, 45, 60, 75, 90
            angle = (float)((value - 4) * 15);

        m_ev?.LockAngle(angle);
    }

    public void OnToggleGrid(ChangeEvent<bool> evt)
    {
        m_gm?.ToggleGrid(evt.newValue);
    }

    public void OnLoadMap(IMapLoader loader, string name)
    {
        Debug.Log("OnLoadMap");
        m_em?.LoadMap(loader, name);
    }

    public void OnWriteMap(IMapWriter writer, string name)
    {
        m_em?.WriteMap(writer, name);
    }

    public void OnCenterView()
    {
        m_ev?.CenterView();
    }

    public void OnFitView()
    {
        m_ev?.FitViewToWindow();        
    }
    
    #endregion

    #region outgoing events

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

    public void NotifyLockAngleListeners(float value)
    {
        m_lockAngle = value;
        int intvalue;
        if (value >= 120f) //120
            intvalue = 11;
        else if (value >= 15f) //15, 30, 45, 60, 75, 90
            intvalue = (int)(value / 15) + 4;
        else if (value >= 5f) //5, 10
            intvalue = (int)(value / 5) + 2;
        else //1, 2
            intvalue = (int)value;

        foreach (var listener in m_lockAngleListeners)
            listener.value = intvalue;
    }

    public void NotifySetModeListeners(EditorStatus.Mode value)
    {
        OnModeChanged?.Invoke(value);

        m_setMode = value;
        foreach (var listener in m_setModeListeners)
            listener.index = (int)value;
    }

    public void NotifySetConstructionModeListeners(EditorStatus.Construct value)
    {
        m_setConstructionMode = value;
        OnConstructionModeChanged?.Invoke(value);
    }

    public void NotifyToggleGridListeners(bool value)
    {
        m_toggleGrid = value;
        foreach (var listener in m_toggleGridListeners)
            listener.value = value;
    }

    // unbuffered mouse move related event
    public void NotifyMouseMoveListeners(Vector2 value)
    {
        OnMouseMoved?.Invoke(value);
    }

    // unbuffered mouse move related event
    public void NotifyCursorInfoChangedListeners(CursorInfo value)
    {
        OnCursorInfoChanged?.Invoke(value);
    }

    public void NotifyRegionMeshChangedListeners(Mesh mesh)
    {
        OnRegionMeshChanged?.Invoke(mesh);
    }

    public void NotifyObjectEditListeners(MapObject mapObject, List<string> objectNames)
    {
        OnEditObject?.Invoke(mapObject, objectNames);
    }
    
    public void NotifyVertexEditListeners(Vertex vertex)
    {
        OnEditVertex?.Invoke(vertex);
    }
    
    public void NotifySegmentEditListeners(Segment segment, List<string> segmentNames)
    {
        OnEditSegment?.Invoke(segment, segmentNames);
    }
    
    public void NotifyRegionEditListeners(Region region, List<string> regionNames)
    {
        OnEditRegion?.Invoke(region, regionNames);
    }
    
    public void NotifyWayEditListeners(Way way, List<string> wayNames)
    {
        OnEditWay?.Invoke(way, wayNames);
    }
    
    #endregion

    //Notify all listeners
    public void RefreshListeners()
    {
        NotifyToggleSnappingListeners(m_toggleSnapping);
        NotifyScaleGridListeners(m_scaleGrid);
        NotifyLockAngleListeners(m_lockAngle);
        NotifySetModeListeners(m_setMode);
        NotifySetConstructionModeListeners(m_setConstructionMode);
        NotifyToggleGridListeners(m_toggleGrid);
    }
}