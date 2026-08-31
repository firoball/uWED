using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Ui
{
    public class KeyBinder
    {
        public KeyBinder(VisualElement parent)
        {
            parent?.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.character)
            {
                case '+':
                    EditorEventBus.Instance.ZoomChanged.Raise(true);
                    break;
                case '-': 
                    EditorEventBus.Instance.ZoomChanged.Raise(false);
                    break;
                default:
                    //Debug.Log($"{evt.character} not handled");
                    break;
            }

            switch (evt.keyCode)
            {
                // Editor View 
                case KeyCode.C:
                    EditorEventBus.Instance.CenterView.Raise();
                    break;
                case KeyCode.F:
                    EditorEventBus.Instance.FitViewToWindow.Raise();
                    break;
                case KeyCode.G:
                    EditorEventBus.Instance.ToggleGrid.Raise(null);
                    break;
                case KeyCode.P:
                    EditorEventBus.Instance.ToggleSnapping.Raise(null);
                    break;
                case KeyCode.KeypadPlus:
                    EditorEventBus.Instance.ZoomChanged.Raise(true);
                    break;
                case KeyCode.KeypadMinus:
                    EditorEventBus.Instance.ZoomChanged.Raise(false);
                    break;


                // Mode selection
                case KeyCode.O:
                    EditorEventBus.Instance.ModeChanged.Raise(EditorStatus.Mode.Objects);
                    break;
                case KeyCode.S:
                    EditorEventBus.Instance.ModeChanged.Raise(EditorStatus.Mode.Segments);
                    break;
                case KeyCode.R:
                    EditorEventBus.Instance.ModeChanged.Raise(EditorStatus.Mode.Regions);
                    break;
                case KeyCode.W:
                    EditorEventBus.Instance.ModeChanged.Raise(EditorStatus.Mode.Ways);
                    break;

                default:
                    //Debug.Log($"{evt.keyCode} not handled");
                    break;
            }
        }
    }
}