using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuBinder
{
    private Label m_gridSizeValue;
    public MenuBinder(EditorView ev, VisualElement menu)
    {
        BindEditorMode(ev, menu);
        BindGridControl(ev, menu);

        //Update initial values of all controls
        ev.Interface.RefreshListeners();
    }

    private void BindEditorMode(EditorView ev, VisualElement menu)
    {
        DropdownField editorModes = menu.Q("editorModes") as DropdownField;
        if (editorModes != null)
        {
            ev.Interface.SetModeListeners.Add(editorModes);
            editorModes.RegisterCallback<ChangeEvent<string>>(ev.Interface.OnSetMode);
        }
        else
            Debug.LogError("Element 'editorModes' not found.");
    }
    private void BindGridControl(EditorView ev, VisualElement menu)
    {
        Toggle gridShow = menu.Q("gridShow") as Toggle;
        if (gridShow != null) 
        {
            ev.Interface.ToggleGridListeners.Add(gridShow);
            gridShow.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleGrid);
        }
        else
            Debug.LogError("Element 'gridShow' not found.");

        SliderInt gridSize = menu.Q("gridSize") as SliderInt;
        m_gridSizeValue = menu.Q("gridSizeValue") as Label;
        if (gridSize != null && m_gridSizeValue != null)
        {
            ev.Interface.ScaleGridListeners.Add(gridSize);
            gridSize.RegisterCallback<ChangeEvent<int>>(OnGridSizeChange);
            gridSize.RegisterCallback<ChangeEvent<int>>(ev.Interface.OnScaleGrid);
            m_gridSizeValue.text = (1 << gridSize.value).ToString();
        }
        else
            Debug.LogError("Element 'gridSize' or 'gridSizeValue' not found.");

        Toggle gridSnap = menu.Q("gridSnap") as Toggle;
        if (gridSnap != null) 
        {
            ev.Interface.ToggleSnappingListeners.Add(gridSnap);
            gridSnap.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleSnapping);
        }
        else
            Debug.LogError("Element 'gridSnap' not found.");
    }

    private void OnGridSizeChange(ChangeEvent<int> evt)
    {
        if (m_gridSizeValue != null)
            m_gridSizeValue.text = (1 << evt.newValue).ToString();
    }
}