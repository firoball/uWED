using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.UI.Manipulator;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.TemplateEditor
{
    /// <summary>
    /// Field panel for a given list of public Template properties, one field per property chosen by its
    /// type: float and int (NumberStepperField, like the Manipulators' numeric fields), string (sanitized
    /// live with NameSanitizer), enum and bool (a bool is a compact toggle). Reads and writes
    /// TemplateEditSession.Current, so it shows the original until the session is editing and then edits
    /// the working copy. Whether editing is allowed at all (locking, Edit/Apply buttons) is up to the host:
    /// the panel only applies a change while the session IsEditing. Free of UnityEditor, and styled with the
    /// shared manip-* classes (ManipulatorBase.uss) and the button class of AcknexManipulatorStyles, which a
    /// host outside a Manipulator must load itself.
    /// </summary>
    public class TemplatePropertyPanel : VisualElement
    {
        readonly List<Action> m_refreshers = new();
        readonly List<PropertyInfo> m_properties = new();
        readonly List<PropertyInfo> m_boolProperties = new();
        readonly List<NumberStepperField> m_floatSteppers = new();
        readonly List<NumberStepperField> m_integerSteppers = new();
        TemplateEditSession m_session;
        bool m_refreshing;

        /// <summary>Builds one field for each named public read/write property of templateType, in the
        /// given order. Names that don't resolve to a supported property are logged and skipped. With
        /// columns above 1 the fields flow in that many columns. With clearAllButton a button above the
        /// fields sets every bool property of the panel to false.</summary>
        public TemplatePropertyPanel(Type templateType, IReadOnlyList<string> propertyNames, int columns = 1, bool clearAllButton = false)
        {
            if (columns > 1)
            {
                style.flexDirection = FlexDirection.Row;
                style.flexWrap = Wrap.Wrap;
            }

            foreach (var propertyName in propertyNames)
            {
                var info = templateType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (info == null || !info.CanRead || !info.CanWrite)
                {
                    Debug.LogWarning($"TemplatePropertyPanel: {templateType.Name} has no public read/write property '{propertyName}'.");
                    continue;
                }

                var element = BuildField(info);
                if (element == null)
                {
                    Debug.LogWarning($"TemplatePropertyPanel: property '{propertyName}' has unsupported type {info.PropertyType.Name}.");
                    continue;
                }

                if (columns > 1)
                    element.style.width = Length.Percent(100f / columns);

                Add(element);
                m_properties.Add(info);
                if (info.PropertyType == typeof(bool))
                    m_boolProperties.Add(info);
            }

            if (clearAllButton)
                Insert(0, BuildClearAllButton());
        }

        /// <summary>Shows and edits session's Template (null unbinds). Registers this panel's properties with
        /// the session so Apply writes them back.</summary>
        public void Bind(TemplateEditSession session)
        {
            if (m_session != null)
                m_session.StateChanged -= Refresh;

            m_session = session;
            if (m_session == null)
                return;

            foreach (var property in m_properties)
                m_session.Track(property);

            m_session.StateChanged += Refresh;
            Refresh();
        }

        /// <summary>The steppers of the int properties. They step by 1; a host that applies its own step to
        /// steppers can use this list to exclude them.</summary>
        public IReadOnlyList<NumberStepperField> IntegerSteppers => m_integerSteppers;

        /// <summary>Sets the step of the float fields. Integer fields always step by 1.</summary>
        public void SetStep(float step)
        {
            foreach (var stepper in m_floatSteppers)
                stepper.Step = step;
        }

        /// <summary>Reloads every field from the session's current Template without raising change events.</summary>
        public void Refresh()
        {
            if (m_session == null)
                return;

            m_refreshing = true;
            foreach (var refresh in m_refreshers)
                refresh();
            m_refreshing = false;
        }

        VisualElement BuildField(PropertyInfo info)
        {
            Type type = info.PropertyType;
            if (type == typeof(bool)) return BuildToggle(info);
            if (type == typeof(float)) return BuildNumberRow(info, isInteger: false);
            if (type == typeof(int)) return BuildNumberRow(info, isInteger: true);
            if (type == typeof(string)) return BuildTextRow(info);
            if (type.IsEnum) return BuildRow(info, new EnumField((Enum)Activator.CreateInstance(type)));
            return null;
        }

        VisualElement BuildClearAllButton()
        {
            var row = new VisualElement();
            row.style.width = Length.Percent(100);
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 4;

            var button = new Button(ClearAllBools) { text = "Clear all" };
            button.AddToClassList(AcknexManipulatorStyles.TemplateBarButton);
            row.Add(button);
            return row;
        }

        void ClearAllBools()
        {
            if (m_session == null || !m_session.IsEditing)
                return;

            bool changed = false;
            foreach (var property in m_boolProperties)
            {
                if (!(bool)property.GetValue(m_session.Current))
                    continue;

                m_session.NotifyEditing();
                property.SetValue(m_session.Current, false);
                changed = true;
            }

            if (!changed)
                return;

            m_session.NotifyEdited();
            Refresh();
        }

        VisualElement BuildToggle(PropertyInfo info)
        {
            var toggle = new Toggle { text = info.Name };
            Wire(info, toggle);
            return toggle;
        }

        VisualElement BuildRow<TValue>(PropertyInfo info, BaseField<TValue> field)
        {
            var row = BuildLabeledRow(info, field);
            Wire(info, field);
            return row;
        }

        static VisualElement BuildLabeledRow(PropertyInfo info, VisualElement field)
        {
            var row = new VisualElement();
            row.AddToClassList("manip-field-row");

            var label = new Label(info.Name);
            label.AddToClassList("manip-field-label");
            row.Add(label);

            field.style.flexGrow = 1;
            row.Add(field);
            return row;
        }

        /// <summary>Number row: a NumberStepperField. A float property's step is set through SetStep; an
        /// int property always steps by 1 (see IntegerSteppers).</summary>
        VisualElement BuildNumberRow(PropertyInfo info, bool isInteger)
        {
            var stepper = new NumberStepperField();
            if (isInteger)
            {
                stepper.Step = 1f;
                m_integerSteppers.Add(stepper);
            }
            else
            {
                m_floatSteppers.Add(stepper);
            }

            stepper.ValueChanged += value =>
            {
                if (m_refreshing || m_session == null || !m_session.IsEditing)
                    return;

                m_session.NotifyEditing();
                info.SetValue(m_session.Current, isInteger ? Mathf.RoundToInt(value) : (object)value);
                m_session.NotifyEdited();
            };

            m_refreshers.Add(() => stepper.Value = Convert.ToSingle(info.GetValue(m_session.Current)));
            return BuildLabeledRow(info, stepper);
        }

        /// <summary>Text row: input is run through NameSanitizer on every change.</summary>
        VisualElement BuildTextRow(PropertyInfo info)
        {
            var field = new TextField();
            field.RegisterValueChangedCallback(evt =>
            {
                string sanitized = NameSanitizer.Sanitize(evt.newValue);
                if (sanitized != evt.newValue)
                    field.SetValueWithoutNotify(sanitized);

                if (m_session == null || !m_session.IsEditing)
                    return;

                if (sanitized == ReadValue<string>(info))
                    return;

                m_session.NotifyEditing();
                info.SetValue(m_session.Current, sanitized);
                m_session.NotifyEdited();
            });

            m_refreshers.Add(() => field.SetValueWithoutNotify(ReadValue<string>(info)));
            return BuildLabeledRow(info, field);
        }

        void Wire<TValue>(PropertyInfo info, BaseField<TValue> field)
        {
            field.RegisterValueChangedCallback(evt =>
            {
                if (m_session == null || !m_session.IsEditing)
                    return;

                m_session.NotifyEditing();
                info.SetValue(m_session.Current, evt.newValue);
                m_session.NotifyEdited();
            });

            m_refreshers.Add(() => field.SetValueWithoutNotify(ReadValue<TValue>(info)));
        }

        TValue ReadValue<TValue>(PropertyInfo info)
        {
            object value = info.GetValue(m_session.Current);
            if (value == null)
                return typeof(TValue) == typeof(string) ? (TValue)(object)string.Empty : default;

            return (TValue)value;
        }
    }
}
