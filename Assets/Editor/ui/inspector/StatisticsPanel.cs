using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// The always-on stats row (objects/vertices/segments/regions/ways,
    /// plus x/y and a status label pinned to the right). Fully independent
    /// of InfoPanel - not nested inside it, loads its own stylesheet
    /// (StatisticsPanel.uss, must sit next to this script). Labels are
    /// created once in the constructor; SetStats/SetExtra only update text.
    /// </summary>
    [UxmlElement]
    public partial class StatisticsPanel : VisualElement
    {
        [SerializeField]
        private StyleSheet m_styleSheet;

        readonly Label _statObjects, _statVertices, _statSegments, _statRegions, _statWays;
        readonly Label _x, _y, _status;

        public StatisticsPanel()
        {
            LoadStyleSheet();
            AddToClassList("statistics-panel");

            _statObjects = AddStatEntry();
            _statVertices = AddStatEntry();
            _statSegments = AddStatEntry();
            _statRegions = AddStatEntry();
            _statWays = AddStatEntry();

            _x = AddStatEntry();
            _x.AddToClassList("info-stat-entry--push");

            _y = AddStatEntry();

            _status = AddStatEntry();
            _status.AddToClassList("info-stat-status");
        }

        Label AddStatEntry()
        {
            var entry = new Label();
            entry.AddToClassList("info-stat-entry");
            Add(entry);
            return entry;
        }

        public void SetStats(InfoPanelStats stats)
        {
            _statObjects.text = $"{stats.TotalObjects} objects";
            _statVertices.text = $"{stats.TotalVertices} vertices";
            _statSegments.text = $"{stats.TotalSegments} segments";
            _statRegions.text = $"{stats.TotalRegions} regions";
            _statWays.text = $"{stats.TotalWays} ways";
            // Add another AddStatEntry() call + a line here for future counts (e.g. textures).
        }

        public void SetExtra(float x, float y, string status)
        {
            _x.text = $"x {FormatFloat(x)}";
            _y.text = $"y {FormatFloat(y)}";
            _status.text = status ?? string.Empty;
        }

        /// <summary>
        /// Loads StatisticsPanel.uss from the same folder as this script -
        /// see InfoPanel.LoadStyleSheet for the same pattern.
        /// </summary>
        void LoadStyleSheet([CallerFilePath] string sourceFilePath = "")
        {
            var directory = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrEmpty(directory))
                return;

            var ussPath = Path.Combine(directory, "StatisticsPanel.uss").Replace('\\', '/');
            var dataPath = Application.dataPath;
            if (!ussPath.StartsWith(dataPath))
            {
                Debug.LogWarning($"StatisticsPanel: script path '{ussPath}' is outside Assets/, cannot resolve stylesheet automatically.");
                return;
            }

            var assetPath = "Assets" + ussPath.Substring(dataPath.Length);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"StatisticsPanel: could not find stylesheet at '{assetPath}'. Panel will render unstyled.");
        }
        
        private static string FormatFloat(float value) => value.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
