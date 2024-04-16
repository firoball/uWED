using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBinder
{
    private Label m_gridSizeValue;
    public MenuBinder(EditorView ev, VisualElement menu)
    {
        IEnumerable<VisualElement> containers = menu.Children();

        VisualElement editorMode = containers.Where(x => x.name == "editorMode").FirstOrDefault();
        BindEditorMode(ev, editorMode);

        VisualElement editorView = containers.Where(x => x.name == "editorView").FirstOrDefault();
        VisualElement gridControl = containers.Where(x => x.name == "gridControl").FirstOrDefault();
        BindGridControl(ev, gridControl);
        
        IEnumerable<VisualElement> manipulators = ev.Children();
        VisualElement gridManipulator = manipulators.Where(x => x.name == "editorMode").FirstOrDefault();

    }

    private void BindEditorMode(EditorView ev, VisualElement editorMode)
    {
        IEnumerable<VisualElement> controls = editorMode.Children();
        DropdownField editorModes = controls.Where(x => x.name == "editorModes").FirstOrDefault() as DropdownField;
        editorModes?.RegisterCallback<ChangeEvent<string>>(ev.EditorManipulator.OnSetMode);
    }
    private void BindGridControl(EditorView ev, VisualElement gridControl)
    {
        IEnumerable<VisualElement> controls = gridControl.Children();

        Toggle gridShow = controls.Where(x => x.name == "gridShow").FirstOrDefault() as Toggle;
        if (gridShow != null) 
        {
            ev.GridManipulator.AddToggleListener(gridShow);
            gridShow.RegisterCallback<ChangeEvent<bool>>(ev.GridManipulator.OnToggleGrid);
        }
        else
            Debug.Log("gridShow NULL");

        SliderInt gridSize = controls.Where(x => x.name == "gridSize").FirstOrDefault() as SliderInt;
        m_gridSizeValue = controls.Where(x => x.name == "gridSizeValue").FirstOrDefault() as Label;

        if (gridSize != null && m_gridSizeValue != null)
        {
            ev.GridManipulator.AddScaleListener(gridSize);
            gridSize.RegisterCallback<ChangeEvent<int>>(OnGridSizeChange);
            gridSize.RegisterCallback<ChangeEvent<int>>(ev.GridManipulator.OnScaleGrid);
            m_gridSizeValue.text = (1 << gridSize.value).ToString();
        }
        else
            Debug.Log("gridSize or m_gridSizeValue NULL");

        Toggle gridSnap = controls.Where(x => x.name == "gridSnap").FirstOrDefault() as Toggle;
        if (gridSnap != null) 
        {
            gridSnap.RegisterCallback<ChangeEvent<bool>>(ev.OnToggleSnapping);
        }
        else
            Debug.Log("gridShow NULL");
    }

    private void OnGridSizeChange(ChangeEvent<int> evt)
    {
        if (m_gridSizeValue != null)
            m_gridSizeValue.text = (1 << evt.newValue).ToString();
    }
}