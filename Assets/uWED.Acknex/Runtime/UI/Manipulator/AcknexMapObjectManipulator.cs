using System;
using System.Collections.Generic;
using UI.Controls;
using UnityEngine.UIElements;
using uWED.Acknex.Runtime.Model;
using uWED.Acknex.Runtime.Model.Templates;
using uWED.Acknex.Runtime.Registry;
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
    /// </summary>
    public class AcknexMapObjectManipulator : MapObjectManipulator
    {
        /// <summary>Name every Player object gets on write-back.</summary>
        public const string PlayerName = "player";

        readonly ObjectTemplatePicker<ThingTemplate> m_thingPicker;
        readonly ObjectTemplatePicker<ActorTemplate> m_actorPicker;

        // Template Name last selected per kind, collected when switching away from a kind and valid until
        // the next LoadValues.
        readonly Dictionary<AcknexObjectType, string> m_rememberedNames = new();

        AcknexObjectType m_activeType;
        NameTextureSlot m_slot;
        MapObject m_loadedCopy;

        /// <summary>Picker of the active kind, null for Player.</summary>
        IObjectTemplatePicker ActivePicker => m_activeType switch
        {
            AcknexObjectType.Thing => m_thingPicker,
            AcknexObjectType.Actor => m_actorPicker,
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

            m_thingPicker = new ObjectTemplatePicker<ThingTemplate>(thingRegistry, thingResolver, RefreshTextureInfo);
            m_actorPicker = new ObjectTemplatePicker<ActorTemplate>(actorRegistry, actorResolver, RefreshTextureInfo);

            // Both pickers take the hidden Name field's place: the base class's only ComboBoxField is its
            // Name combo, whose parent is the Name section.
            var content = this.Q<VisualElement>("manip-content-container");
            int nameSectionIndex = content.IndexOf(content.Q<ComboBoxField>().parent);
            content.Insert(nameSectionIndex, m_thingPicker.Root);
            content.Insert(nameSectionIndex + 1, m_actorPicker.Root);
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

            base.LoadValues(copy);

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
        }

        /// <inheritdoc/>
        protected override void OnTypeChanged(int newTypeId)
        {
            var previousPicker = ActivePicker;
            if (previousPicker?.Current != null)
                m_rememberedNames[m_activeType] = previousPicker.Current.Name;

            m_activeType = ToObjectType(newTypeId);

            ActivePicker?.Select(m_rememberedNames.TryGetValue(m_activeType, out var name) ? name : null);
            ApplyPickerVisibility();
            RefreshTextureInfo();
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
