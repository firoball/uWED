using System;
using System.Globalization;
using UI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Runtime.Core.Utilities;
using uWED.Runtime.Platform;
using uWED.Runtime.UI.EventBus;
using uWED.Runtime.UI.Help;
using uWED.Runtime.UI.View2D;

namespace uWED.Runtime.UI.Binder
{
    public class MenuBinder
    {
        private Label m_gridSizeValue;
        private Label m_angleSizeValue;

        public MenuBinder(EditorView ev, EditorHelp eh, VisualElement menuParent, Action onEditorClose)
        {
            BindFileMenu(menuParent, onEditorClose);
            BindEditorMode(ev, eh, menuParent);
            BindSnapControl(ev, menuParent);
            BindHelpButton(eh, menuParent);

        }

        private void BindFileMenu(VisualElement parent, Action onEditorClose)
        {
            IFileDialog fileDialog = ServiceLocator.Get<IFileDialog>();
            if (parent.Q("fileMenu") is MenuButton fileMenu && fileDialog != null)
            {
                fileMenu.menu.AppendAction("New", null);
                fileMenu.menu.AppendAction("Load", fileDialog.Load);
                fileMenu.menu.AppendAction("Save", fileDialog.Save);
                fileMenu.menu.AppendAction("Save as...", fileDialog.SaveAs);
                fileMenu.menu.AppendSeparator();
                fileMenu.menu.AppendAction("Exit", (x) => onEditorClose());
            }
            else
                Debug.LogError("Element 'fileMenu' not found.");
        }

        private void BindEditorMode(EditorView ev, EditorHelp eh, VisualElement parent)
        {
            if (parent.Q("editorModes") is DropdownField editorModes)
            {
                EditorEventBus.Instance.ModeChanged.Subscribe(v => editorModes.index = (int)v);
                editorModes.RegisterCallback<ChangeEvent<string>>(ev.Interface.OnSetMode);
                editorModes.RegisterCallback<ChangeEvent<string>>(eh.OnSetMode);
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
                EditorEventBus.Instance.LockAngle.Subscribe(v => angleSize.value = LockAngleUtility.DegreesToIndex(v));
                angleSize.RegisterCallback<ChangeEvent<int>>(ev.Interface.OnLockAngle);
                angleSize.RegisterCallback<ChangeEvent<int>>(OnAngleSizeChange);
                m_angleSizeValue.text = LockAngleUtility.IndexToDegrees(angleSize.value).ToString(CultureInfo.InvariantCulture);
            }
            else
                Debug.LogError("Element 'angleSize' or 'angleSizeValue' not found.");


            Toggle gridShow = parent.Q("gridShow") as Toggle;
            if (gridShow != null) 
            {
                EditorEventBus.Instance.ToggleGrid.Subscribe(v => gridShow.value = v ?? !gridShow.value);
                gridShow.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleGrid);
            }
            else
                Debug.LogError("Element 'gridShow' not found.");

            SliderInt gridSize = parent.Q("gridSize") as SliderInt;
            m_gridSizeValue = parent.Q("gridSizeValue") as Label;
            if (gridSize != null && m_gridSizeValue != null)
            {
                EditorEventBus.Instance.ScaleGrid.Subscribe(v => gridSize.value = (int)v);
                gridSize.RegisterCallback<ChangeEvent<int>>(ev.Interface.OnScaleGrid);
                gridSize.RegisterCallback<ChangeEvent<int>>(OnGridSizeChange);
                m_gridSizeValue.text = FormatGridSizeValue(gridSize.value);
            }
            else
                Debug.LogError("Element 'gridSize' or 'gridSizeValue' not found.");

            Toggle enableSnap = parent.Q("enableSnap") as Toggle;
            if (enableSnap != null) 
            {
                EditorEventBus.Instance.ToggleSnapping.Subscribe(v => enableSnap.value = v ?? !enableSnap.value);
                enableSnap.RegisterCallback<ChangeEvent<bool>>(ev.Interface.OnToggleSnapping);
            }
            else
                Debug.LogError("Element 'enableSnap' not found.");
        }

        private void BindHelpButton(EditorHelp eh, VisualElement parent)
        {
            Button helpButton = parent.Q("help") as Button;
            DropdownField editorModes = parent.Q("editorModes") as DropdownField;

            if (helpButton != null && editorModes != null)
            {
                helpButton.RegisterCallback<ClickEvent>(eh.OnOpenHelp);
            }
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
                m_angleSizeValue.text = LockAngleUtility.IndexToDegrees(evt.newValue).ToString(CultureInfo.InvariantCulture);
        }

    }
}