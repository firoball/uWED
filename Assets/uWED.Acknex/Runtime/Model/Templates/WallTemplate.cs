namespace uWED.Acknex.Runtime.Model.Templates
{
    /// <summary>
    /// Acknex Wall properties, on top of the Wall/Thing/Actor-shared set on BaseObjectTemplate (see
    /// wall.md). Wall has no properties of its own beyond that shared set - only its own flags.
    /// </summary>
    public class WallTemplate : BaseObjectTemplate
    {
        /// <summary>Transparent flag.</summary>
        public bool Transparent { get => m_flags.IsSet(AcknexFlag.Transparent); set => m_flags = value ? m_flags.Set(AcknexFlag.Transparent) : m_flags.Reset(AcknexFlag.Transparent); }
        /// <summary>Curtain flag.</summary>
        public bool Curtain { get => m_flags.IsSet(AcknexFlag.Curtain); set => m_flags = value ? m_flags.Set(AcknexFlag.Curtain) : m_flags.Reset(AcknexFlag.Curtain); }
        /// <summary>Portcullis flag.</summary>
        public bool Portcullis { get => m_flags.IsSet(AcknexFlag.Portcullis); set => m_flags = value ? m_flags.Set(AcknexFlag.Portcullis) : m_flags.Reset(AcknexFlag.Portcullis); }
        /// <summary>Fence flag.</summary>
        public bool Fence { get => m_flags.IsSet(AcknexFlag.Fence); set => m_flags = value ? m_flags.Set(AcknexFlag.Fence) : m_flags.Reset(AcknexFlag.Fence); }
    }
}
