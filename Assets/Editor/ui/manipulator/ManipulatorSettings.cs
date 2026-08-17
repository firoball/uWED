using UnityEditor;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// EditorPrefs-backed IManipulatorSettings. This is the one deliberate
    /// UnityEditor dependency in the whole feature - EditorPrefs has no
    /// engine-side equivalent, everything else stays UnityEngine.UIElements.
    /// One shared instance should be passed into every *Manipulator window.
    /// </summary>
    public class ManipulatorSettings : IManipulatorSettings
    {
        const string LinearStepKey = "Manipulator.LinearStep";
        const string AngleStepKey = "Manipulator.AngleStep";

        const float DefaultLinearStep = 0.1f;
        const float DefaultAngleStep = 15f;

        public float LinearStep
        {
            get => EditorPrefs.GetFloat(LinearStepKey, DefaultLinearStep);
            set => EditorPrefs.SetFloat(LinearStepKey, value);
        }

        public float AngleStep
        {
            get => EditorPrefs.GetFloat(AngleStepKey, DefaultAngleStep);
            set => EditorPrefs.SetFloat(AngleStepKey, value);
        }
    }
}
