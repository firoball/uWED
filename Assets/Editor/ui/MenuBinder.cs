using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBinder
{
    private Label m_gridSizeValue;
    private Label m_angleSizeValue;

    public MenuBinder(EditorView ev, VisualElement parent, EditorWindow wnd)
    {
        BindFileMenu(ev, parent, wnd);
        BindEditorMode(ev, parent);
        BindSnapControl(ev, parent);

        //Update initial values of all controls
        ev.Interface.RefreshListeners();
    }

    private void BindFileMenu(EditorView ev, VisualElement parent, EditorWindow wnd)
    {
        ToolbarMenu toolbarMenu = parent.Q("fileMenu") as ToolbarMenu;
        if (toolbarMenu != null && ev != null)
        {
            FileDialog fileDialog = new FileDialog(ev.Interface);
            toolbarMenu.menu.AppendAction("New", null);
            toolbarMenu.menu.AppendAction("Load", fileDialog.Load);
            toolbarMenu.menu.AppendAction("Save", fileDialog.Save);
            toolbarMenu.menu.AppendAction("Save as...", fileDialog.SaveAs);
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("Exit", (x) => wnd?.Close());
        }
        else
            Debug.LogError("Element 'fileMenu' not found.");
    }

    private void BindEditorMode(EditorView ev, VisualElement parent)
    {
        DropdownField editorModes = parent.Q("editorModes") as DropdownField;
        if (editorModes != null)
        {
            ev.Interface.SetModeListeners.Add(editorModes);
            editorModes.RegisterCallback<ChangeEvent<string>>(ev.Interface.OnSetMode);
        }
        else
            Debug.LogError("Element 'editorModes' not found.");
    }
    private void BindSnapControl(EditorView ev, VisualElement parent)
    {
        SliderInt angleSize = parent.Q("angleSize") as SliderInt;
        m_angleSizeValue = parent.Q("angleSizeValue") as Label;
        if (angleSize != null && m_angleSizeValue != null)
        {
            ev.Interface.LockAngleListeners.Add(angleSize);
            angleSize.RegisterCallback<ChangeEvent<int>>(OnAngleSizeChange);
            angleSize.RegisterCallback<ChangeEvent<int>>(ev.Interface.OnLockAngle);
            m_angleSizeValue.text = FormatAngleSizeValue(angleSize.value);
        }
        else
            Debug.LogError("Element 'angleSize' or 'angleSizeValue' not found.");


        Toggle gridShow = parent.Q("gridShow") as Toggle;
        if (gridShow != null) 
        {
            ev.Interface.ToggleGridListeners.Add(gridShow);
            gridShow.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleGrid);
        }
        else
            Debug.LogError("Element 'gridShow' not found.");

        SliderInt gridSize = parent.Q("gridSize") as SliderInt;
        m_gridSizeValue = parent.Q("gridSizeValue") as Label;
        if (gridSize != null && m_gridSizeValue != null)
        {
            ev.Interface.ScaleGridListeners.Add(gridSize);
            gridSize.RegisterCallback<ChangeEvent<int>>(OnGridSizeChange);
            gridSize.RegisterCallback<ChangeEvent<int>>(ev.Interface.OnScaleGrid);
            m_gridSizeValue.text = FormatGridSizeValue(gridSize.value);
        }
        else
            Debug.LogError("Element 'gridSize' or 'gridSizeValue' not found.");

        Toggle enableSnap = parent.Q("enableSnap") as Toggle;
        if (enableSnap != null) 
        {
            ev.Interface.ToggleSnappingListeners.Add(enableSnap);
            enableSnap.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleSnapping);
        }
        else
            Debug.LogError("Element 'enableSnap' not found.");
    }

    private void OnGridSizeChange(ChangeEvent<int> evt)
    {
        if (m_gridSizeValue != null)
            m_gridSizeValue.text = FormatGridSizeValue(evt.newValue);
    }

    private string FormatGridSizeValue(int value)
    {
        return (1 << value).ToString();
    }

    private void OnAngleSizeChange(ChangeEvent<int> evt)
    {
        if (m_angleSizeValue != null)
            m_angleSizeValue.text = FormatAngleSizeValue(evt.newValue);
    }

    private string FormatAngleSizeValue(int value)
    {
        if (value == 1 || value == 2) //1, 2
            return value.ToString();
        else if (value == 3 || value == 4) //5, 10
            return ((value - 2) * 5).ToString();
        else if (value == 11) //120
            return 120.ToString();
        else //15, 30, 45, 60, 75, 90
            return ((value - 4) * 15).ToString();
    }

}