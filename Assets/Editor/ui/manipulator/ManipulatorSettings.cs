using Runtime.Platform;
using UnityEngine;

namespace Editor.UI.Manipulator
{
    /// <summary>
    /// IManipulatorSettings backed by cached fields, persisted through an
    /// IPrefsProvider. One shared instance should be passed into every
    /// *Manipulator window. Values are only read from/written to the
    /// provider in OnLoadPrefs/OnSavePrefs - LinearStep/AngleStep just
    /// return the cached field the rest of the time.
    /// </summary>
    public class ManipulatorSettings : IManipulatorSettings
    {
        const string LinearStepKey = "uWED::ManipulatorSettings::LinearStep";
        const string AngleStepKey = "uWED::ManipulatorSettings::AngleStep";

        const float DefaultLinearStep = 0.1f;
        const float DefaultAngleStep = 15f;

        private float m_linearStep = DefaultLinearStep;
        private float m_angleStep = DefaultAngleStep;

        public ManipulatorSettings()
        {
            EditorEventBus.Instance.LoadPrefs.Subscribe(OnLoadPrefs);
            EditorEventBus.Instance.LoadPrefs.Subscribe(OnSavePrefs);
        }
        
        public float LinearStep
        {
            get => m_linearStep;
            set => m_linearStep = value;
        }

        public float AngleStep
        {
            get => m_angleStep;
            set => m_angleStep = value;
        }


        public void OnSavePrefs(IPrefsProvider prefsProvider)
        {
            if (prefsProvider == null)
            {
                Debug.LogWarning("ManipulatorSettings.OnSavePrefs: no IPrefsProvider set, skipping save.");
                return;
            }

            prefsProvider.SetFloat(LinearStepKey, m_linearStep);
            prefsProvider.SetFloat(AngleStepKey, m_angleStep);
        }

        public void OnLoadPrefs(IPrefsProvider prefsProvider)
        {
            if (prefsProvider == null)
            {
                Debug.LogWarning("ManipulatorSettings.OnLoadPrefs: no IPrefsProvider set, skipping load.");
                return;
            }

            m_linearStep = prefsProvider.GetFloat(LinearStepKey, DefaultLinearStep);
            m_angleStep = prefsProvider.GetFloat(AngleStepKey, DefaultAngleStep);
        }
    }
}
