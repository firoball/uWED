using System;
using System.Collections.Generic;
using UI.Controls;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
using uWED.Acknex.Runtime.UI.TemplateEditor;
using uWED.Runtime.Core.Map.Model;
using uWED.Runtime.UI.Manipulator;

namespace uWED.Acknex.Runtime.UI.Manipulator
{
    /// <summary>
    /// MapObject Manipulator for Acknex Things, Actors and the Player. The object kind is the base
    /// window's type selector (IndexedData.TypeId, values of AcknexObjectType, supplied by
    /// AcknexManipulatorTypeProvider); an unlisted TypeId is treated as Thing.
    /// Thing and Actor replace the base Name field with a Template picker for the matching Template type.
    /// The link to a Template is by Name only (see TemplateResolver). Switching the kind never carries a
    /// Name over to the other Template type: the target picker restores the Template last selected for
    /// that kind during the current Open(), or else selects that type's default Template. Player has no
    /// Template and no picker; its Name is always "player".
    /// Thing and Actor also add their Template tabs (see TemplateTabSet) to the window; only the active
    /// kind's tabs are present, and switching the kind drops pending Template edits.
    /// </summary>
    public class AcknexMapObjectManipulator : MapObjectManipulator
    {
        /// <summary>Name every Player object gets on write-back.</summary>
        public const string PlayerName = "player";

        const string UsageNoun = "object";

        readonly ObjectTemplatePicker<ThingTemplate> m_thingPicker;
        readonly ObjectTemplatePicker<ActorTemplate> m_actorPicker;
        readonly TemplateTabSet<ThingTemplate> m_thingTabs;
        readonly TemplateTabSet<ActorTemplate> m_actorTabs;

        // Template Name last selected per kind, collected when switching away from a kind and valid until
        // the next LoadValues.
        readonly Dictionary<AcknexObjectType, string> m_rememberedNames = new();

        AcknexObjectType m_activeType;
        ITemplateTabSet m_shownTabs;

        // Object name -> number of MapObjects carrying it, as last handed to SetCountByName.
        IReadOnlyDictionary<string, int> m_countByName;
        NameTextureSlot m_slot;
        MapObject m_loadedCopy;

        /// <summary>Picker of the active kind, null for Player.</summary>
        IObjectTemplatePicker ActivePicker => m_activeType switch
        {
            AcknexObjectType.Thing => m_thingPicker,
            AcknexObjectType.Actor => m_actorPicker,
            _ => null,
        };

        /// <summary>Template tabs of the active kind, null for Player.</summary>
        ITemplateTabSet ActiveTabs => m_activeType switch
        {
            AcknexObjectType.Thing => m_thingTabs,
            AcknexObjectType.Actor => m_actorTabs,
            _ => null,
        };

        /// <summary>Builds the window; each registry backs its picker's choices and "+" clone, each
        /// resolver maps an object's Name to a Template (creating the type's default if none matches).</summary>
        public AcknexMapObjectManipulator(
            VisualTreeAsset baseUxml,
            IManipulatorSettings settings,
            TemplateRegistry<ThingTemplate> thingRegistry,
            TemplateResolver<ThingTemplate> thingResolver,
            TemplateRegistry<ActorTemplate> actorRegistry,
            TemplateResolver<ActorTemplate> actorResolver)
            : base(baseUxml, settings)
        {
            AcknexManipulatorStyles.ApplyTo(this);
            SetNameFieldVisible(false);

            m_thingPicker = new ObjectTemplatePicker<ThingTemplate>(thingRegistry, thingResolver, OnTemplateSelectionChanged);
            m_actorPicker = new ObjectTemplatePicker<ActorTemplate>(actorRegistry, actorResolver, OnTemplateSelectionChanged);

            m_thingTabs = new TemplateTabSet<ThingTemplate>(thingRegistry, thingResolver, ThingTemplateLayout.Groups, UsageNoun);
            m_actorTabs = new TemplateTabSet<ActorTemplate>(actorRegistry, actorResolver, ActorTemplateLayout.Groups, UsageNoun);
            WireTabs(m_thingTabs, m_thingPicker);
            WireTabs(m_actorTabs, m_actorPicker);

            // Both pickers take the hidden Name field's place: the base class's only ComboBoxField is its
            // Name combo, whose parent is the Name section.
            var content = this.Q<VisualElement>("manip-content-container");
            int nameSectionIndex = content.IndexOf(content.Q<ComboBoxField>().parent);
            content.Insert(nameSectionIndex, m_thingPicker.Root);
            content.Insert(nameSectionIndex + 1, m_actorPicker.Root);
        }

        /// <summary>Stores the object name usage table for the usage text shown on the Template tabs.</summary>
        public override void SetCountByName(IReadOnlyDictionary<string, int> countByName)
        {
            m_countByName = countByName;
        }

        /// <summary>Connects a tab set to its picker: "Clone &amp; Edit" selects a copy and edits it, the
        /// picker is locked while editing, and a change of the Template refreshes the picker's detail rows.</summary>
        void WireTabs(ITemplateTabSet tabs, IObjectTemplatePicker picker)
        {
            tabs.CloneRequested += () =>
            {
                picker.CloneCurrent();
                tabs.BeginEdit();
            };
            tabs.EditingChanged += editing => picker.SetEnabled(!editing);
            tabs.Changed += picker.Refresh;

            foreach (var tab in tabs.Tabs)
            {
                foreach (var stepper in tab.IntegerSteppers)
                    stepper.AddToClassList(FixedStepStepperClass);
            }
        }

        void OnTemplateSelectionChanged()
        {
            RefreshTextureInfo();
            BindActiveTabs();
        }

        /// <summary>Binds the active kind's tabs to the Template selected in its picker.</summary>
        void BindActiveTabs()
        {
            string openName = OriginalTarget.Name;
            switch (m_activeType)
            {
                case AcknexObjectType.Thing:
                    m_thingTabs.Bind(m_thingPicker.Selected, openName, m_countByName, CurrentLinearStep);
                    break;
                case AcknexObjectType.Actor:
                    m_actorTabs.Bind(m_actorPicker.Selected, openName, m_countByName, CurrentLinearStep);
                    break;
            }
        }

        /// <summary>Puts exactly the active kind's Template tabs into the window.</summary>
        void ShowActiveTabs()
        {
            var active = ActiveTabs;
            if (ReferenceEquals(m_shownTabs, active))
                return;

            if (m_shownTabs != null)
            {
                foreach (var tab in m_shownTabs.Tabs)
                    tab.RemoveFromHierarchy();
            }

            if (active != null)
            {
                foreach (var tab in active.Tabs)
                    TabView.Add(tab);
            }

            m_shownTabs = active;
        }

        /// <inheritdoc/>
        protected override void LoadValues(MapObject copy)
        {
            m_loadedCopy = copy;
            m_rememberedNames.Clear();
            m_activeType = ToObjectType(EditedTypeId);

            bool wasUnnamed = string.IsNullOrEmpty(copy.Name);

            // Selected before base.LoadValues, which calls LoadTextureInfo and needs the Template.
            ActivePicker?.Select(copy.Name);
            ApplyPickerVisibility();
            ShowActiveTabs();

            base.LoadValues(copy);
            BindActiveTabs();

            // An unnamed Thing/Actor only makes sense tied to a Template, so the resolved default is
            // committed immediately.
            if (wasUnnamed && ActivePicker != null)
                ApplyNow();
        }

        /// <inheritdoc/>
        protected override void WriteBack(MapObject target, MapObject editedCopy)
        {
            base.WriteBack(target, editedCopy);

            var picker = ActivePicker;
            if (m_activeType == AcknexObjectType.Player)
                target.Name = PlayerName;
            else if (picker?.Current != null)
                target.Name = picker.Current.Name;

            ActiveTabs?.Apply(); // confirming the window also applies pending Template edits
        }

        /// <inheritdoc/>
        protected override void OnTypeChanged(int newTypeId)
        {
            ActiveTabs?.Discard();

            var previousPicker = ActivePicker;
            if (previousPicker?.Current != null)
                m_rememberedNames[m_activeType] = previousPicker.Current.Name;

            m_activeType = ToObjectType(newTypeId);

            ActivePicker?.Select(m_rememberedNames.TryGetValue(m_activeType, out var name) ? name : null);
            ApplyPickerVisibility();
            RefreshTextureInfo();
            ShowActiveTabs();
            BindActiveTabs();
        }

        /// <summary>Shows the active kind's Texture in the base texture slot ("-" for Player).</summary>
        protected override void LoadTextureInfo(NameTextureSlot slot, MapObject copy)
        {
            m_slot = slot;
            m_loadedCopy = copy;

            var texture = ActivePicker?.Current?.Texture;
            slot.TextureNameValue.text = texture?.Name ?? "-";
            slot.ScaleValue.text = texture?.Template != null
                ? $"Scale X/Y: {texture.Template.Scale_x:0.###} / {texture.Template.Scale_y:0.###}"
                : "Scale X/Y: -";
        }

        void RefreshTextureInfo()
        {
            if (m_slot != null && m_loadedCopy != null)
                LoadTextureInfo(m_slot, m_loadedCopy);
        }

        void ApplyPickerVisibility()
        {
            m_thingPicker.Root.style.display = m_activeType == AcknexObjectType.Thing ? DisplayStyle.Flex : DisplayStyle.None;
            m_actorPicker.Root.style.display = m_activeType == AcknexObjectType.Actor ? DisplayStyle.Flex : DisplayStyle.None;
        }

        static AcknexObjectType ToObjectType(int typeId)
            => Enum.IsDefined(typeof(AcknexObjectType), typeId) ? (AcknexObjectType)typeId : AcknexObjectType.Thing;
    }
}
