using System.Collections.Generic;
using UnityEngine;
using uWED.Acknex.Runtime.Model.Assets;
using uWED.Acknex.Runtime.Model.Instances;
using Font = uWED.Acknex.Runtime.Model.Assets.Font;

namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Acknex Texture properties (see templates/texture.md). Multi-slot properties (Bmaps/Delay/Mirror/
    /// Offset_x/Offset_y/Scycles) are data-driven lists rather than fixed-size arrays - their length
    /// follows Sides, not a compile-time constant. `Oneshot` has no property here - it's hidden/
    /// runtime-only, not settable at definition time. Has no Manipulator/Inspector UI - Texture's
    /// editing UI is built separately from the other Template types' shared Manipulator popup.
    /// </summary>
    public class TextureTemplate : TemplateAsset
    {
        [SerializeField] int m_sides;
        [SerializeField] int m_cycles;
        [SerializeField] int m_frame;
        [SerializeField] List<Bmap> m_bmaps = new();
        [SerializeField] Flic m_flic;
        [SerializeField] string m_title;
        [SerializeField] Assets.Model m_model;
        [SerializeField] List<float> m_delay = new();
        [SerializeField] List<bool> m_mirror = new();
        [SerializeField] List<float> m_offsetX = new();
        [SerializeField] List<float> m_offsetY = new();
        [SerializeField] float m_random;
        [SerializeField] float m_scaleX;
        [SerializeField] float m_scaleY;
        [SerializeField] float m_ambient;
        [SerializeField] float m_albedo;
        [SerializeField] float m_radiance;
        [SerializeField] Sound m_sound;
        [SerializeField] float m_svol;
        [SerializeField] float m_sdist;
        [SerializeField] float m_svdist;
        [SerializeField] List<float> m_scycles = new();
        [SerializeField] float m_scycle;
        [SerializeReference] TextureInstance m_attach; // plain-class self-reference - see TextureInstance's own doc comment
        [SerializeField] float m_posX;
        [SerializeField] float m_posY;
        [SerializeField] string m_touch;
        [SerializeField] Font m_font;
        [SerializeField] string m_ifTouch;
        [SerializeField] string m_ifRelease;
        [SerializeField] string m_ifKlick;
        [SerializeField] uint m_flags;

        /// <summary>Number of animation frame slots - drives the length of Bmaps/Delay/Mirror/Offset_x/Offset_y.</summary>
        public int Sides { get => m_sides; set => m_sides = value; }
        /// <summary>Number of animation cycles to play.</summary>
        public int Cycles { get => m_cycles; set => m_cycles = value; }
        /// <summary>Current/starting frame index.</summary>
        public int Frame { get => m_frame; set => m_frame = value; }
        /// <summary>One Bmap per animation frame slot, length = Sides.</summary>
        public List<Bmap> Bmaps { get => m_bmaps; set => m_bmaps = value; }
        /// <summary>Flic animation this Texture plays, if any.</summary>
        public Flic Flic { get => m_flic; set => m_flic = value; }
        /// <summary>Display title.</summary>
        public string Title { get => m_title; set => m_title = value; }
        /// <summary>3D model this Texture represents, if any.</summary>
        public Assets.Model Model { get => m_model; set => m_model = value; }
        /// <summary>Per-frame delay before advancing to the next slot, length = Sides.</summary>
        public List<float> Delay { get => m_delay; set => m_delay = value; }
        /// <summary>Per-frame horizontal-mirror flag, length = Sides.</summary>
        public List<bool> Mirror { get => m_mirror; set => m_mirror = value; }
        /// <summary>Per-frame horizontal offset, length = Sides.</summary>
        public List<float> Offset_x { get => m_offsetX; set => m_offsetX = value; }
        /// <summary>Per-frame vertical offset, length = Sides.</summary>
        public List<float> Offset_y { get => m_offsetY; set => m_offsetY = value; }
        /// <summary>Random animation-timing variance.</summary>
        public float Random { get => m_random; set => m_random = value; }
        /// <summary>Horizontal scale.</summary>
        public float Scale_x { get => m_scaleX; set => m_scaleX = value; }
        /// <summary>Vertical scale.</summary>
        public float Scale_y { get => m_scaleY; set => m_scaleY = value; }
        /// <summary>Ambient light contribution.</summary>
        public float Ambient { get => m_ambient; set => m_ambient = value; }
        /// <summary>Surface albedo.</summary>
        public float Albedo { get => m_albedo; set => m_albedo = value; }
        /// <summary>Emitted light radiance.</summary>
        public float Radiance { get => m_radiance; set => m_radiance = value; }
        /// <summary>Sound played by this Texture, if any.</summary>
        public Sound Sound { get => m_sound; set => m_sound = value; }
        /// <summary>Sound volume.</summary>
        public float Svol { get => m_svol; set => m_svol = value; }
        /// <summary>Sound falloff distance.</summary>
        public float Sdist { get => m_sdist; set => m_sdist = value; }
        /// <summary>Sound variable/randomized distance.</summary>
        public float Svdist { get => m_svdist; set => m_svdist = value; }
        /// <summary>Per-frame sound-cycle timing, length = Sides.</summary>
        public List<float> Scycles { get => m_scycles; set => m_scycles = value; }
        /// <summary>Single sound-cycle timing value, distinct from the per-frame Scycles list.</summary>
        public float Scycle { get => m_scycle; set => m_scycle = value; }
        /// <summary>Another Texture this one attaches to. Self-referential - a resolver walking this chain needs a cycle guard.</summary>
        public TextureInstance Attach { get => m_attach; set => m_attach = value; }
        /// <summary>Horizontal position.</summary>
        public float Pos_x { get => m_posX; set => m_posX = value; }
        /// <summary>Vertical position.</summary>
        public float Pos_y { get => m_posY; set => m_posY = value; }
        /// <summary>Touch interaction label.</summary>
        public string Touch { get => m_touch; set => m_touch = value; }
        /// <summary>Font this Texture renders with, if any.</summary>
        public Font Font { get => m_font; set => m_font = value; }
        /// <summary>WDL function/label name invoked on touch.</summary>
        public string If_touch { get => m_ifTouch; set => m_ifTouch = value; }
        /// <summary>WDL function/label name invoked on release.</summary>
        public string If_release { get => m_ifRelease; set => m_ifRelease = value; }
        /// <summary>WDL function/label name invoked on klick.</summary>
        public string If_klick { get => m_ifKlick; set => m_ifKlick = value; }

        /// <summary>Ghost flag.</summary>
        public bool Ghost { get => m_flags.IsSet(AcknexFlag.Ghost); set => m_flags = value ? m_flags.Set(AcknexFlag.Ghost) : m_flags.Reset(AcknexFlag.Ghost); }
        /// <summary>Diaphanous flag.</summary>
        public bool Diaphanous { get => m_flags.IsSet(AcknexFlag.Diaphanous); set => m_flags = value ? m_flags.Set(AcknexFlag.Diaphanous) : m_flags.Reset(AcknexFlag.Diaphanous); }
        /// <summary>Behind flag.</summary>
        public bool Behind { get => m_flags.IsSet(AcknexFlag.Behind); set => m_flags = value ? m_flags.Set(AcknexFlag.Behind) : m_flags.Reset(AcknexFlag.Behind); }
        /// <summary>Shadow flag.</summary>
        public bool Shadow { get => m_flags.IsSet(AcknexFlag.Shadow); set => m_flags = value ? m_flags.Set(AcknexFlag.Shadow) : m_flags.Reset(AcknexFlag.Shadow); }
        /// <summary>Lightmap flag.</summary>
        public bool Lightmap { get => m_flags.IsSet(AcknexFlag.Lightmap); set => m_flags = value ? m_flags.Set(AcknexFlag.Lightmap) : m_flags.Reset(AcknexFlag.Lightmap); }
        /// <summary>Sky flag.</summary>
        public bool Sky { get => m_flags.IsSet(AcknexFlag.Sky); set => m_flags = value ? m_flags.Set(AcknexFlag.Sky) : m_flags.Reset(AcknexFlag.Sky); }
        /// <summary>Wire flag.</summary>
        public bool Wire { get => m_flags.IsSet(AcknexFlag.Wire); set => m_flags = value ? m_flags.Set(AcknexFlag.Wire) : m_flags.Reset(AcknexFlag.Wire); }
        /// <summary>Cluster flag.</summary>
        public bool Cluster { get => m_flags.IsSet(AcknexFlag.Cluster); set => m_flags = value ? m_flags.Set(AcknexFlag.Cluster) : m_flags.Reset(AcknexFlag.Cluster); }
        /// <summary>No_clip flag.</summary>
        public bool No_clip { get => m_flags.IsSet(AcknexFlag.No_clip); set => m_flags = value ? m_flags.Set(AcknexFlag.No_clip) : m_flags.Reset(AcknexFlag.No_clip); }
        /// <summary>Clip flag.</summary>
        public bool Clip { get => m_flags.IsSet(AcknexFlag.Clip); set => m_flags = value ? m_flags.Set(AcknexFlag.Clip) : m_flags.Reset(AcknexFlag.Clip); }
        /// <summary>Sloop flag.</summary>
        public bool Sloop { get => m_flags.IsSet(AcknexFlag.Sloop); set => m_flags = value ? m_flags.Set(AcknexFlag.Sloop) : m_flags.Reset(AcknexFlag.Sloop); }
        /// <summary>Condensed flag.</summary>
        public bool Condensed { get => m_flags.IsSet(AcknexFlag.Condensed); set => m_flags = value ? m_flags.Set(AcknexFlag.Condensed) : m_flags.Reset(AcknexFlag.Condensed); }
        /// <summary>Narrow flag.</summary>
        public bool Narrow { get => m_flags.IsSet(AcknexFlag.Narrow); set => m_flags = value ? m_flags.Set(AcknexFlag.Narrow) : m_flags.Reset(AcknexFlag.Narrow); }
        /// <summary>Save flag.</summary>
        public bool Save { get => m_flags.IsSet(AcknexFlag.Save); set => m_flags = value ? m_flags.Set(AcknexFlag.Save) : m_flags.Reset(AcknexFlag.Save); }
    }
}
