using System;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.UI.TemplateEditor;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// Header of a Manipulator's Template tabs: shows the Template's name and how many instances share it,
    /// notes that edits affect all of them, and guards editing behind an explicit "Edit" button beside the
    /// text. "Clone & Edit" asks the host (CloneRequested) for a copy of the Template, which the host then
    /// opens for editing. While editing the bar offers Apply (enabled once a field changed) and Discard. This protection is specific to
    /// the Manipulator window; other hosts of TemplatePropertyPanel (Inspector, a Template editor) edit
    /// without it.
    /// </summary>
    public class TemplateEditBar : VisualElement
    {
        const string SharedNote = "Changes apply to all instances. Use Clone & Edit to change only this one.";

        readonly Label m_usage;
        readonly Button m_cloneAndEdit;
        readonly Button m_edit;
        readonly Button m_apply;
        readonly Button m_discard;
        TemplateEditSession m_session;

        /// <summary>Raised when "Clone & Edit" is pressed. The host creates the copy and starts editing it.</summary>
        public event Action CloneRequested;

        /// <summary>Builds the bar with its usage text, note and buttons; call Bind to attach a session.</summary>
        public TemplateEditBar()
        {
            AddToClassList(AcknexManipulatorStyles.TemplateBar);

            var header = new VisualElement();
            header.AddToClassList(AcknexManipulatorStyles.TemplateBarHeader);

            m_usage = new Label();
            m_usage.AddToClassList(AcknexManipulatorStyles.TemplateBarUsage);
            header.Add(m_usage);

            var buttons = new VisualElement();
            buttons.AddToClassList(AcknexManipulatorStyles.TemplateBarButtons);
            m_cloneAndEdit = BuildButton("Clone & Edit", () => CloneRequested?.Invoke(), buttons);
            m_edit = BuildButton("Edit", () => m_session?.BeginEdit(), buttons);
            m_apply = BuildButton("Apply", () => m_session?.Apply(), buttons);
            m_discard = BuildButton("Discard", () => m_session?.Discard(), buttons);
            header.Add(buttons);
            Add(header);

            var note = new Label(SharedNote);
            note.AddToClassList(AcknexManipulatorStyles.TemplateBarNote);
            Add(note);
        }

        /// <summary>Shows the session's Template name followed by usageText (e.g. "used by 10 walls") and
        /// drives session; null disables the bar.</summary>
        public void Bind(TemplateEditSession session, string usageText)
        {
            if (m_session != null)
            {
                m_session.StateChanged -= UpdateButtons;
                m_session.Edited -= UpdateButtons;
            }

            m_session = session;
            m_usage.text = BuildHeaderText(session?.Original.Name, usageText);

            if (m_session != null)
            {
                m_session.StateChanged += UpdateButtons;
                m_session.Edited += UpdateButtons;
            }

            UpdateButtons();
        }

        void UpdateButtons()
        {
            bool hasSession = m_session != null;
            bool editing = hasSession && m_session.IsEditing;

            m_cloneAndEdit.style.display = editing ? DisplayStyle.None : DisplayStyle.Flex;
            m_cloneAndEdit.SetEnabled(hasSession);
            m_edit.style.display = editing ? DisplayStyle.None : DisplayStyle.Flex;
            m_edit.SetEnabled(hasSession);
            m_apply.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
            m_apply.SetEnabled(editing && m_session.IsDirty);
            m_discard.style.display = editing ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static string BuildHeaderText(string templateName, string usageText)
        {
            if (string.IsNullOrEmpty(usageText))
                return templateName ?? string.Empty;

            return string.IsNullOrEmpty(templateName) ? usageText : $"{templateName} · {usageText}";
        }

        static Button BuildButton(string text, Action onClick, VisualElement parent)
        {
            var button = new Button(onClick) { text = text };
            button.AddToClassList(AcknexManipulatorStyles.TemplateBarButton);
            parent.Add(button);
            return button;
        }
    }
}
