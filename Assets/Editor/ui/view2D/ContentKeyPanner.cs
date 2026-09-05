using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.View2D
{
    public class ContentKeyPanner : KeyboardNavigationManipulator
    {
        private static float s_moveFactor = 1;
        
        // Distance (in pixels) moved per navigation operation.
        // Repeat rate while a key is held follows the OS key-repeat setting.
        private const float c_moveDist = 64f;

        private const float c_moveFactor = 5f;

        public ContentKeyPanner() : base(Apply)
        {
        }

        protected override void RegisterCallbacksOnTarget()
        {
            if (target is not GridView)
            {
                throw new InvalidOperationException("Manipulator can only be added to a GridView");
            }

            // Keyboard events require the target to be focusable.
            target.focusable = true;

            // Give the GridView keyboard focus as soon as the cursor enters it,
            // so arrow-key panning works without requiring a prior click.
            target.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<KeyUpEvent>(OnKeyUp);
            
            base.RegisterCallbacksOnTarget();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<KeyUpEvent>(OnKeyUp);

            base.UnregisterCallbacksFromTarget();
        }

        private void OnMouseEnter(MouseEnterEvent e)
        {
            target.Focus();
        }

        private void OnKeyDown(KeyDownEvent e)
        {
            if (e.keyCode is KeyCode.LeftControl or KeyCode.RightControl)
                s_moveFactor = c_moveFactor;
        }

        private void OnKeyUp(KeyUpEvent e)
        {
            if (e.keyCode is KeyCode.LeftControl or KeyCode.RightControl)
                s_moveFactor = 1;
        }

        private static void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
        {
            if (sourceEvent?.target is not GridView gridView)
                return;

            Vector2 dir = Vector2.zero;

            switch (op)
            {
                case KeyboardNavigationOperation.Previous:
                    dir.y = 1f;
                    break;
                case KeyboardNavigationOperation.Next:
                    dir.y = -1f;
                    break;
                case KeyboardNavigationOperation.MoveLeft:
                    dir.x = 1f;
                    break;
                case KeyboardNavigationOperation.MoveRight:
                    dir.x = -1f;
                    break;
                default:
                    // Not a pan operation (e.g. Submit, Cancel, PageUp/Down, Begin/End) -
                    // let it propagate normally instead of consuming it here.
                    return;
            }

            Vector2 diff = dir * c_moveDist * s_moveFactor;

            Vector3 s = gridView.contentViewContainer.resolvedStyle.scale.value;
            Vector3 p = gridView.contentViewContainer.resolvedStyle.translate + Vector3.Scale(diff, s);

            gridView.contentViewContainer.style.translate = p;
            gridView.UpdateViewTransform(p, s);

            sourceEvent.StopPropagation();
        }
    }
}