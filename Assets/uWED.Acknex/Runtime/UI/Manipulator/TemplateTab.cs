using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// One Manipulator tab showing a group of a Template's properties: a TemplateEditBar above a
    /// TemplatePropertyPanel. The panel is disabled until the bound session is editing. All tabs of a
    /// window are bound to the same session, so Edit/Apply/Discard on any of them acts on all.
    /// </summary>
    public class TemplateTab : Tab
    {
        readonly TemplateEditBar m_bar = new();
        readonly TemplatePropertyPanel m_panel;
        TemplateEditSession m_session;

        /// <summary>Raised when the tab's "Clone & Edit" button is pressed.</summary>
        public event Action CloneRequested;

        /// <summary>Creates a tab titled like group showing its properties of templateType.</summary>
        public TemplateTab(Type templateType, TemplatePropertyGroup group)
        {
            label = group.Title;

            m_panel = new TemplatePropertyPanel(templateType, group.Properties, group.Columns, group.ClearAllButton);
            m_bar.CloneRequested += () => CloneRequested?.Invoke();

            var scroll = new ScrollView();
            RemoveFromFocusOrder(scroll);
            scroll.Add(m_bar);
            scroll.Add(m_panel);
            Add(scroll);

            m_panel.SetEnabled(false);
        }

        /// <summary>The steppers of the tab's int properties (see TemplatePropertyPanel.IntegerSteppers).</summary>
        public IReadOnlyList<NumberStepperField> IntegerSteppers => m_panel.IntegerSteppers;

        /// <summary>Sets the step of the tab's float fields (see TemplatePropertyPanel.SetStep).</summary>
        public void SetStep(float step) => m_panel.SetStep(step);

        /// <summary>Shows and edits session's Template (null unbinds); usageText is shown in the bar.</summary>
        public void Bind(TemplateEditSession session, string usageText)
        {
            if (m_session != null)
                m_session.StateChanged -= UpdateEnabled;

            m_session = session;
            if (m_session != null)
                m_session.StateChanged += UpdateEnabled;

            m_bar.Bind(session, usageText);
            m_panel.Bind(session);
            UpdateEnabled();
        }

        /// <summary>Takes the scroll view and its scrollbars (sliders and buttons) out of the Tab order, so
        /// only the controls placed in it are Tab stops.</summary>
        static void RemoveFromFocusOrder(ScrollView scroll)
        {
            scroll.focusable = false;
            scroll.Query<Scroller>().ForEach(scroller =>
            {
                scroller.focusable = false;
                scroller.Query<VisualElement>().ForEach(element => element.focusable = false);
            });
        }

        void UpdateEnabled() => m_panel.SetEnabled(m_session != null && m_session.IsEditing);
    }
}
