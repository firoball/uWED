using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EditorManipulator : MouseManipulator
{
    private MapData m_mapData; //TODO: move to EditorView
    private MapDrawer m_drawer; //TODO: move to EditorView
    private Label m_mouseLabel;
    private EditorMode.Construct m_constructMode;
    private EditorMode.Mode m_mode;
    private List<IEditorMode> m_editorModes;

    public EditorManipulator()
    {
        //TEMP - this should be handled in a better way
        m_mapData = AssetDatabase.LoadAssetAtPath<MapData>("assets/testmap.asset");
        if (m_mapData == null)
        {
            m_mapData = ScriptableObject.CreateInstance<MapData>();
            string path = AssetDatabase.GenerateUniqueAssetPath("assets/testmap.asset");
            AssetDatabase.CreateAsset(m_mapData, path);
            //if (m_mapData == null) //everything failed... at least allow using the editor
            //    m_mapData = new MapData();
        }

        m_drawer = new MapDrawer(m_mapData);
        m_mouseLabel = new Label { name = "mousePosition", text = "(0,0)" };

        m_editorModes = new List<IEditorMode>
        {
            new ObjectMode(m_mapData, m_drawer),
            new SegmentMode(m_mapData, m_drawer),
            new RegionMode(m_mapData, m_drawer),
            new WayMode(m_mapData, m_drawer)
        };

        m_mode = EditorMode.Mode.Segments;

        m_constructMode = EditorMode.Construct.Idle;

        /*
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse });
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.RightMouse, modifiers = EventModifiers.Control });
        */
    }
    protected override void RegisterCallbacksOnTarget()
    {
        target.Add(m_drawer);
        target.Add(m_mouseLabel);
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseCaptureOutEvent>(OnMouseOut);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseOut);
    }

    private void OnMouseOut(MouseCaptureOutEvent evt)
    {

    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        var t = target as EditorView;
        m_mouseLabel.transform.position = evt.localMousePosition + Vector2.up * 20;
        m_mouseLabel.text = t.ScreenToWorldSpace(evt.localMousePosition).ToString() + " " + m_constructMode;
        //m_mouseLabel.text = t.ScreenToWorldSpace(evt.localMousePosition).ToString();

        m_drawer.SetLocalMousePosition(evt.localMousePosition);

        if (m_constructMode == EditorMode.Construct.Idle)
        {
            CursorInfo ci = m_drawer.CursorInfo;
            //any mouse button pressed
            if ((evt.pressedButtons & 7) != 0)
            {
                if (ci.HoverVertex != null || ci.HoverSegment != null)
                    m_constructMode = EditorMode.Construct.Dragging;
                m_drawer.MarkDirtyRepaint();
            }
            //if left mouse is pressed 
            if ((evt.pressedButtons & 1) != 0)
            {
                if (ci.HoverVertex != null || ci.HoverSegment != null)
                {
                    m_editorModes[(int)m_mode].StartDrag(m_drawer.CursorInfo);
                }
                else
                {
                    if (!evt.ctrlKey) // ctrl key allows multi select
                        m_drawer.Unselect();
                    m_constructMode = EditorMode.Construct.Selecting;
                    m_drawer.SetSelectMode(true);
                }

            }
        }
        else if (m_constructMode == EditorMode.Construct.Selecting)
        {
            //if left mouse is pressed 
            if ((evt.pressedButtons & 1) != 0)
            {
                m_drawer.SetSelectMode(true);
            }
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        //if (!CanStopManipulation(evt)) return;
        target.ReleaseMouse();

        var t = target as EditorView;
        Vector2 mouseWorldPos = t.ScreenToWorldSpace(evt.localMousePosition);
        Vector2 mouseSnappedWorldPos = t.SnapWorldPos(mouseWorldPos);
        CursorInfo ci = m_drawer.CursorInfo;

        switch (m_constructMode)
        {
            case EditorMode.Construct.Idle: //start construction
                {
                    switch (evt.button)
                    {
                        case 0: //left mousebutton
                            if (ci.SelectionIsActive)
                            {
                                m_drawer.Unselect();
                            }
                            else if (!evt.ctrlKey) //ctrl triggers selection, don't start construction
                            {
                                if(!m_editorModes[(int)m_mode].StartConstruction(ci, mouseSnappedWorldPos))
                                    m_constructMode = EditorMode.Construct.Constructing;
                            }
                            else
                            {
                                //TODO: select
                            }
                            break;

                        case 1: //right mousebutton
                            if (!evt.ctrlKey)
                                m_editorModes[(int)m_mode].EditObject(ci);
                            else
                                m_editorModes[(int)m_mode].DeleteObject(ci);
                            break;

                        case 2: //middle mousebutton
                            if (!evt.ctrlKey)
                                m_editorModes[(int)m_mode].ModifyObject(ci, mouseWorldPos, t);
                            else
                                m_editorModes[(int)m_mode].ModifyObjectAlt(ci, mouseWorldPos, t);
                            break;

                        default:
                            break;
                    }
                }
                break;

            case EditorMode.Construct.Constructing: //in construction
                if (evt.button == 0) //left mousebutton
                {
                    if (m_editorModes[(int)m_mode].ProgressConstruction(ci, mouseSnappedWorldPos))
                        m_constructMode = EditorMode.Construct.Idle; // construction finished
                }
                else if (evt.button == 1) //right mousebutton
                {
                    if(m_editorModes[(int)m_mode].RevertConstruction())
                        m_constructMode = EditorMode.Construct.Idle; // construction aborted
                }
                break;

            case EditorMode.Construct.Dragging: // finish drag mode
                if (evt.button == 0) //left mousebutton
                    m_editorModes[(int)m_mode].FinishDrag(ci, mouseSnappedWorldPos);
                m_constructMode = EditorMode.Construct.Idle;
                break;

            case EditorMode.Construct.Selecting: // finish selection mode
                if (evt.button == 0) //left mousebutton
                {
                    m_drawer.SetSelectMode(false);
                    m_constructMode = EditorMode.Construct.Idle;
                }
                break;

            default:
                break;
        }

        m_drawer.MarkDirtyRepaint();
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        //if (!CanStartManipulation(evt)) return;
        target.CaptureMouse();
        m_drawer.MarkDirtyRepaint();
    }

    public void OnSetMode(ChangeEvent<string> evt)
    {
        PopupField<string> field = evt.target as PopupField<string>;
        if (field != null && field.index >= 0 && field.index < (int)EditorMode.Mode.Count)
            m_mode = (EditorMode.Mode)field.index;
        //TODO: proper mode transfer (esp. end ongoing constructions and drags)
    }

/*
    private List<BaseField<string>> m_modeListeners = new List<BaseField<string>>();
    public void AddModeListener(BaseField<string> strField)
    {
        if (strField != null)
        {
            m_modeListeners.Add(strField);
            UpdateModeListeners();
        }
    }

    private void UpdateModeListeners()
    {
    }*/


}