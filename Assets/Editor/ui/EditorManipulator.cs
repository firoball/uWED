using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EditorManipulator : MouseManipulator
{
    private MapData m_mapData; //TODO: move to EditorView?
    private Label m_mouseLabel;
    private EditorStatus.Construct m_constructMode;
    private EditorStatus.Mode m_mode;
    private List<BaseEditorMode> m_editorModes;

    private MapManager m_mapManager;
    private const string c_defaultAsset = "assets/DefaultMapAsset.asset";

    public EditorManipulator()
    {
        m_mapManager = new MapManager();
        m_mapManager.Load(new MapAssetLoader(), c_defaultAsset);
        //m_mapManager.CreateMap("assets/testmap.asset");
        m_mapData = m_mapManager.MapData; //TEMP
        //TEMP - this should be handled in a better way
/*
        m_mapData = AssetDatabase.LoadAssetAtPath<MapData>("assets/testmap.asset");
        if (m_mapData == null)
        {
            m_mapData = ScriptableObject.CreateInstance<MapData>();
            string path = AssetDatabase.GenerateUniqueAssetPath("assets/testmap.asset");
            AssetDatabase.CreateAsset(m_mapData, path);
            //if (m_mapData == null) //everything failed... at least allow using the editor
            //    m_mapData = new MapData();
        }
*/
        m_mouseLabel = new Label { name = "mousePosition", text = "(0,0)", pickingMode = PickingMode.Ignore };


        m_editorModes = new List<BaseEditorMode>
        {
            new ObjectMode(m_mapData),
            new SegmentMode(m_mapData),
            new RegionMode(m_mapData),
            new WayMode(m_mapData)
        };

        m_mode = EditorStatus.Mode.Segments;
        m_constructMode = EditorStatus.Construct.Idle;
        LoadPrefs();
        ResetMode();
    }

    protected override void RegisterCallbacksOnTarget()
    {
        //pass defaults to all listeners
        EditorView ev = target as EditorView;
        m_editorModes[(int)m_mode].Drawer.SetEnabled(true);
        ev?.Interface.NotifySetModeListeners(m_mode);

        //add all editor mode drawers as children
        foreach (BaseEditorMode em in m_editorModes)
        {
            if (em.Drawer != null)
                target.Add(em.Drawer);
        }
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

        m_editorModes[(int)m_mode].Drawer.SetLocalMousePosition(evt.localMousePosition);

        if (m_constructMode == EditorStatus.Construct.Idle)
        {
            //middle mouse button pressed
            if ((evt.pressedButtons & 4) != 0) //Content Dragger is active
            {
                m_constructMode = EditorStatus.Construct.Moving;
            }
            //if left mouse is pressed 
            else if ((evt.pressedButtons & 1) != 0)
            {
                if (m_editorModes[(int)m_mode].StartDrag(evt.ctrlKey))
                {
                    m_constructMode = EditorStatus.Construct.Dragging;
                }
                else
                {
                    if (!evt.ctrlKey) // ctrl key allows multi select
                        m_editorModes[(int)m_mode].ClearSelection();
                    m_editorModes[(int)m_mode].StartSelection();
                    m_constructMode = EditorStatus.Construct.Selecting;
                }

            }
            else 
            {
                //do nothing
            }
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        target.ReleaseMouse();

        var t = target as EditorView;
        Vector2 mouseWorldPos = t.ScreenToWorldSpace(evt.localMousePosition);
        Vector2 mouseSnappedWorldPos = t.SnapWorldPos(mouseWorldPos);

        switch (m_constructMode)
        {
            case EditorStatus.Construct.Idle: //start construction
                {
                    switch (evt.button)
                    {
                        case 0: //left mousebutton
                            if (!evt.ctrlKey) //ctrl triggers selection, don't start construction
                            {
                                if (!m_editorModes[(int)m_mode].ClearSelection()) //clear current selection
                                {
                                    if (!m_editorModes[(int)m_mode].StartConstruction(mouseSnappedWorldPos))
                                        m_constructMode = EditorStatus.Construct.Constructing;
                                }
                            }
                            else
                            {
                                //select single element, add to current selection
                                m_editorModes[(int)m_mode].SingleSelection();
                            }
                            break;

                        case 1: //right mousebutton
                            if (!evt.ctrlKey)
                                m_editorModes[(int)m_mode].EditObject();
                            else
                                m_editorModes[(int)m_mode].DeleteObject();
                            break;

                        case 2: //middle mousebutton
                            if (!evt.ctrlKey)
                                m_editorModes[(int)m_mode].ModifyObject(mouseWorldPos, t);
                            else
                                m_editorModes[(int)m_mode].ModifyObjectAlt(mouseWorldPos, t);
                            break;

                        default:
                            break;
                    }
                }
                break;

            case EditorStatus.Construct.Constructing: //in construction
                if (evt.button == 0) //left mousebutton
                {
                    if (m_editorModes[(int)m_mode].ProgressConstruction(mouseSnappedWorldPos))
                        m_constructMode = EditorStatus.Construct.Idle; // construction finished
                }
                else if (evt.button == 1) //right mousebutton
                {
                    if(m_editorModes[(int)m_mode].RevertConstruction())
                        m_constructMode = EditorStatus.Construct.Idle; // construction aborted
                }
                break;

            case EditorStatus.Construct.Dragging: // finish drag mode
                if (evt.button == 0) //left mousebutton
                {
                    m_editorModes[(int)m_mode].FinishDrag(mouseSnappedWorldPos);
                    m_constructMode = EditorStatus.Construct.Idle;
                }
                else if (evt.button == 1) //right mousebutton
                {
                    m_editorModes[(int)m_mode].AbortDrag();
                    //m_constructMode will be set to idle with release of left mousebutton only
                }
                break;

            case EditorStatus.Construct.Selecting: // finish selection mode
                if (evt.button == 0) //left mousebutton
                {
                    m_editorModes[(int)m_mode].FinishSelection();
                    m_constructMode = EditorStatus.Construct.Idle;
                }
                break;

            case EditorStatus.Construct.Moving: // finish move mode
                if (evt.button == 2) //middle mousebutton
                {
                    m_constructMode = EditorStatus.Construct.Idle;
                }
                break;

            default:
                break;
        }
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        target.CaptureMouse();
    }

    private void ResetMode()
    {
        m_editorModes[(int)m_mode].Initialize();
        m_constructMode = EditorStatus.Construct.Idle;
    }

    public void SetMode(EditorStatus.Mode mode)
    {
        if (mode != m_mode)
        {
//            ResetMode();
            m_editorModes[(int)m_mode].Drawer.SetEnabled(false);
            m_mode = mode;
            ResetMode();
            EditorView ev = target as EditorView;
            ev?.Interface.NotifySetModeListeners(m_mode);
            m_editorModes[(int)m_mode].Drawer.SetEnabled(true);
        }
    }

    public void SavePrefs()
    {
        EditorPrefs.SetFloat("uWED::EditorManipulator::mode", (int)m_mode);
    }

    private void LoadPrefs()
    {
        if (EditorPrefs.HasKey("uWED::EditorManipulator::mode"))
            m_mode = (EditorStatus.Mode)EditorPrefs.GetFloat("uWED::EditorManipulator::mode");
    }


}