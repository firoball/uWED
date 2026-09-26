using UnityEngine;
using uWED.Acknex.Runtime.Model.Instances;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Acknex Region properties (see region.md). Region doesn't share Wall/Thing/Actor's BaseObject-
    /// derived property set at all - only the generic Flag1..Flag8 slots via MapObjectTemplate.
    /// Its lift-related flags include two alias pairs (Floor_ascend/Floor_lifted, Ceil_descend/
    /// Ceil_lifted - each pair is one physical bit under two WDL names) plus one compound flag
    /// (Lifted, which has no bit of its own - see its own doc comment).
    /// </summary>
    public class RegionTemplate : MapObjectTemplate
    {
        [SerializeField] RegionTemplate m_below;
        [SerializeReference] TextureInstance m_floorTex;
        [SerializeReference] TextureInstance m_ceilTex;
        [SerializeReference] TextureInstance m_texture1;
        [SerializeReference] TextureInstance m_texture2;
        [SerializeReference] TextureInstance m_texture3;
        [SerializeReference] TextureInstance m_texture4;
        [SerializeField] ThingTemplate m_genius;
        [SerializeField] float m_floorHgt;
        [SerializeField] float m_ceilHgt;
        [SerializeField] float m_floorAngle;
        [SerializeField] float m_ceilAngle;
        [SerializeField] float m_floorOffsX;
        [SerializeField] float m_floorOffsY;
        [SerializeField] float m_ceilOffsX;
        [SerializeField] float m_ceilOffsY;
        [SerializeField] float m_ambient;
        [SerializeField] float m_clipDist;
        [SerializeField] string m_ifEnter;
        [SerializeField] string m_ifLeave;
        [SerializeField] string m_ifDive;
        [SerializeField] string m_ifArise;
        [SerializeField] string m_eachCycle;
        [SerializeField] string m_eachTick;

        /// <summary>Region below this one.</summary>
        public RegionTemplate Below { get => m_below; set => m_below = value; }
        /// <summary>Floor texture.</summary>
        public TextureInstance Floor_tex { get => m_floorTex; set => m_floorTex = value; }
        /// <summary>Ceiling texture.</summary>
        public TextureInstance Ceil_tex { get => m_ceilTex; set => m_ceilTex = value; }
        /// <summary>Additional texture slot 1.</summary>
        public TextureInstance Texture1 { get => m_texture1; set => m_texture1 = value; }
        /// <summary>Additional texture slot 2.</summary>
        public TextureInstance Texture2 { get => m_texture2; set => m_texture2 = value; }
        /// <summary>Additional texture slot 3.</summary>
        public TextureInstance Texture3 { get => m_texture3; set => m_texture3 = value; }
        /// <summary>Additional texture slot 4.</summary>
        public TextureInstance Texture4 { get => m_texture4; set => m_texture4 = value; }
        /// <summary>The Thing/Actor Template driving this Region's behavior. ActorTemplate : ThingTemplate, so either fits here.</summary>
        public ThingTemplate Genius { get => m_genius; set => m_genius = value; }
        /// <summary>Floor height.</summary>
        public float Floor_hgt { get => m_floorHgt; set => m_floorHgt = value; }
        /// <summary>Ceiling height.</summary>
        public float Ceil_hgt { get => m_ceilHgt; set => m_ceilHgt = value; }
        /// <summary>Floor tilt angle.</summary>
        public float Floor_angle { get => m_floorAngle; set => m_floorAngle = value; }
        /// <summary>Ceiling tilt angle.</summary>
        public float Ceil_angle { get => m_ceilAngle; set => m_ceilAngle = value; }
        /// <summary>Floor texture horizontal offset.</summary>
        public float Floor_offs_x { get => m_floorOffsX; set => m_floorOffsX = value; }
        /// <summary>Floor texture vertical offset.</summary>
        public float Floor_offs_y { get => m_floorOffsY; set => m_floorOffsY = value; }
        /// <summary>Ceiling texture horizontal offset.</summary>
        public float Ceil_offs_x { get => m_ceilOffsX; set => m_ceilOffsX = value; }
        /// <summary>Ceiling texture vertical offset.</summary>
        public float Ceil_offs_y { get => m_ceilOffsY; set => m_ceilOffsY = value; }
        /// <summary>Ambient light level.</summary>
        public float Ambient { get => m_ambient; set => m_ambient = value; }
        /// <summary>Clipping distance.</summary>
        public float Clip_dist { get => m_clipDist; set => m_clipDist = value; }
        /// <summary>WDL function/label name invoked on enter.</summary>
        public string If_enter { get => m_ifEnter; set => m_ifEnter = value; }
        /// <summary>WDL function/label name invoked on leave.</summary>
        public string If_leave { get => m_ifLeave; set => m_ifLeave = value; }
        /// <summary>WDL function/label name invoked on dive.</summary>
        public string If_dive { get => m_ifDive; set => m_ifDive = value; }
        /// <summary>WDL function/label name invoked on arise.</summary>
        public string If_arise { get => m_ifArise; set => m_ifArise = value; }
        /// <summary>WDL function/label name invoked once per cycle. Distinct from the hidden/runtime-only floor/ceiling-specific variants.</summary>
        public string Each_cycle { get => m_eachCycle; set => m_eachCycle = value; }
        /// <summary>WDL function/label name invoked once per engine tick.</summary>
        public string Each_tick { get => m_eachTick; set => m_eachTick = value; }

        /// <summary>Floor ascend flag - the same underlying bit as Floor_lifted; the two are WDL alias names for one flag, always in sync.</summary>
        public bool Floor_ascend { get => m_flags.IsSet(AcknexFlag.Floor_ascend); set => m_flags = value ? m_flags.Set(AcknexFlag.Floor_ascend) : m_flags.Reset(AcknexFlag.Floor_ascend); }
        /// <summary>Ceiling ascend flag.</summary>
        public bool Ceil_ascend { get => m_flags.IsSet(AcknexFlag.Ceil_ascend); set => m_flags = value ? m_flags.Set(AcknexFlag.Ceil_ascend) : m_flags.Reset(AcknexFlag.Ceil_ascend); }
        /// <summary>Floor descend flag.</summary>
        public bool Floor_descend { get => m_flags.IsSet(AcknexFlag.Floor_descend); set => m_flags = value ? m_flags.Set(AcknexFlag.Floor_descend) : m_flags.Reset(AcknexFlag.Floor_descend); }
        /// <summary>Ceiling descend flag - the same underlying bit as Ceil_lifted; the two are WDL alias names for one flag, always in sync.</summary>
        public bool Ceil_descend { get => m_flags.IsSet(AcknexFlag.Ceil_descend); set => m_flags = value ? m_flags.Set(AcknexFlag.Ceil_descend) : m_flags.Reset(AcknexFlag.Ceil_descend); }
        /// <summary>Ceiling lifted flag - the same underlying bit as Ceil_descend; the two are WDL alias names for one flag, always in sync.</summary>
        public bool Ceil_lifted { get => m_flags.IsSet(AcknexFlag.Ceil_lifted); set => m_flags = value ? m_flags.Set(AcknexFlag.Ceil_lifted) : m_flags.Reset(AcknexFlag.Ceil_lifted); }
        /// <summary>Floor lifted flag - the same underlying bit as Floor_ascend; the two are WDL alias names for one flag, always in sync.</summary>
        public bool Floor_lifted { get => m_flags.IsSet(AcknexFlag.Floor_lifted); set => m_flags = value ? m_flags.Set(AcknexFlag.Floor_lifted) : m_flags.Reset(AcknexFlag.Floor_lifted); }
        /// <summary>Compound lift flag with no bit of its own - reads as true only when both Floor_lifted and Ceil_ascend are set, and setting or clearing it sets or clears both of those together. Deliberately pairs with Ceil_ascend rather than Ceil_lifted.</summary>
        public bool Lifted
        {
            get => m_flags.IsSet(AcknexFlag.Floor_lifted) && m_flags.IsSet(AcknexFlag.Ceil_ascend);
            set => m_flags = value
                ? m_flags.Set(AcknexFlag.Floor_lifted).Set(AcknexFlag.Ceil_ascend)
                : m_flags.Reset(AcknexFlag.Floor_lifted).Reset(AcknexFlag.Ceil_ascend);
        }
        /// <summary>Save flag.</summary>
        public bool Save { get => m_flags.IsSet(AcknexFlag.Save); set => m_flags = value ? m_flags.Set(AcknexFlag.Save) : m_flags.Reset(AcknexFlag.Save); }
        /// <summary>Save_all flag.</summary>
        public bool Save_all { get => m_flags.IsSet(AcknexFlag.Save_all); set => m_flags = value ? m_flags.Set(AcknexFlag.Save_all) : m_flags.Reset(AcknexFlag.Save_all); }
        /// <summary>Sticky flag.</summary>
        public bool Sticky { get => m_flags.IsSet(AcknexFlag.Sticky); set => m_flags = value ? m_flags.Set(AcknexFlag.Sticky) : m_flags.Reset(AcknexFlag.Sticky); }
        /// <summary>Base flag.</summary>
        public bool Base { get => m_flags.IsSet(AcknexFlag.Base); set => m_flags = value ? m_flags.Set(AcknexFlag.Base) : m_flags.Reset(AcknexFlag.Base); }
    }
}
