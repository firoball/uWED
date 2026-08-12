using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UI.Inspector
{
    /// <summary>
    /// Bottom-left docked info panel. Every possible view (one per entity
    /// type, per column, plus the multi-select summary) is created exactly
    /// once in the constructor. Calling one of the SetHover/SetSelection
    /// overloads never creates or destroys VisualElements - it only toggles
    /// which pre-built view is visible and updates that view's existing
    /// labels. This matters because hover changes on essentially every
    /// mouse-move frame; rebuilding the subtree each time would mean
    /// constant allocation and layout thrashing.
    ///
    /// Binds directly to the project's own domain types (Vertex, Segment,
    /// Region, MapObject, Way) - no separate DTO layer. Each carries its own
    /// .Index now, so that's read directly rather than passed in. Per-region
    /// segment count and texture names still aren't stored on those types,
    /// so they're passed in as extra (optional) parameters - only relevant
    /// when exactly one item is selected, and still placeholders until a
    /// real texture lookup exists.
    ///
    /// SetSelection takes the whole selected list per domain: pass 0, 1, or
    /// many and the panel decides internally whether to clear, show the
    /// single-item detail, or fall back to the count-only summary - no
    /// count-based branching needed on the caller's side.
    /// </summary>
    [UxmlElement]
    public partial class InfoPanel : VisualElement
    {
        // hover column - only Vertex/Segment/Region/Object can be touched
        readonly VisualElement _hoverColumn;
        readonly VertexDetailView _hoverVertex;
        readonly SegmentDetailView _hoverSegment;
        readonly RegionDetailView _hoverRegion;
        readonly ObjectDetailView _hoverObject;
        readonly VisualElement[] _hoverViews;

        // reserved column for future parameters - see ExtraColumn
        readonly VisualElement _extraColumn;

        // selection column - covers all four domains plus single-item detail
        readonly VisualElement _selectionColumn;
        readonly VertexDetailView _selVertex;
        readonly SegmentDetailView _selSegment;
        readonly RegionDetailView _selRegion;
        readonly ObjectDetailView _selObject;
        readonly WayDetailView _selWay;
        readonly SelectionSummaryView _selSummary;
        readonly VisualElement[] _selectionViews;

        bool _hoverActive;
        bool _selectionActive;

        public VisualElement ExtraColumn => _extraColumn;

        public InfoPanel()
        {
            LoadStyleSheet();
            AddToClassList("info-panel");

            var columns = new VisualElement();
            columns.AddToClassList("info-panel__columns");

            // --- hover column --------------------------------------------
            _hoverColumn = new VisualElement();
            _hoverColumn.AddToClassList("info-panel__column");
            _hoverColumn.AddToClassList("info-panel__hover");

            _hoverVertex = new VertexDetailView();
            _hoverSegment = new SegmentDetailView();
            _hoverRegion = new RegionDetailView();
            _hoverObject = new ObjectDetailView();
            _hoverViews = new VisualElement[] { _hoverVertex, _hoverSegment, _hoverRegion, _hoverObject };
            foreach (var view in _hoverViews)
            {
                view.style.display = DisplayStyle.None;
                _hoverColumn.Add(view);
            }

            // --- reserved extra column ------------------------------------
            _extraColumn = new VisualElement();
            _extraColumn.AddToClassList("info-panel__column");
            _extraColumn.AddToClassList("info-panel__extra");

            // --- selection column -------------------------------------------
            _selectionColumn = new VisualElement();
            _selectionColumn.AddToClassList("info-panel__column");
            _selectionColumn.AddToClassList("info-panel__selection");

            _selVertex = new VertexDetailView();
            _selSegment = new SegmentDetailView();
            _selRegion = new RegionDetailView();
            _selObject = new ObjectDetailView();
            _selWay = new WayDetailView();
            _selSummary = new SelectionSummaryView();
            _selectionViews = new VisualElement[] { _selVertex, _selSegment, _selRegion, _selObject, _selWay, _selSummary };
            foreach (var view in _selectionViews)
            {
                view.style.display = DisplayStyle.None;
                _selectionColumn.Add(view);
            }

            // Selected single-item detail views get a distinct header color
            // so it's visually clear this is a selection, not a hover.
            _selVertex.AddToClassList("info-detail-view--selected");
            _selSegment.AddToClassList("info-detail-view--selected");
            _selRegion.AddToClassList("info-detail-view--selected");
            _selObject.AddToClassList("info-detail-view--selected");
            _selWay.AddToClassList("info-detail-view--selected");

            columns.Add(_hoverColumn);
            columns.Add(_extraColumn);
            columns.Add(_selectionColumn);
            Add(columns);

            _hoverColumn.style.display = DisplayStyle.None;
            _selectionColumn.style.display = DisplayStyle.None;
            UpdatePanelVisibility();
        }

        // === hover =========================================================

        public void SetHover(Vertex v)
        {
            if (v == null) { ClearHover(); return; }
            ShowHoverView(_hoverVertex);
            _hoverVertex.Bind(v);
        }

        public void SetHover(Segment s, string leftTextureName = null, string rightTextureName = null)
        {
            if (s == null) { ClearHover(); return; }
            ShowHoverView(_hoverSegment);
            _hoverSegment.Bind(s, leftTextureName, rightTextureName);
        }

        public void SetHover(Region r, int segmentCount = 0, string floorTextureName = null, string ceilingTextureName = null)
        {
            if (r == null) { ClearHover(); return; }
            ShowHoverView(_hoverRegion);
            _hoverRegion.Bind(r, segmentCount, floorTextureName, ceilingTextureName);
        }

        public void SetHover(MapObject o, string textureName = null)
        {
            if (o == null) { ClearHover(); return; }
            ShowHoverView(_hoverObject);
            _hoverObject.Bind(o, textureName);
        }

        /// <summary>Call when nothing is under the mouse - hides the hover column.</summary>
        public void ClearHover()
        {
            _hoverActive = false;
            _hoverColumn.style.display = DisplayStyle.None;
            HideAll(_hoverViews);
            UpdatePanelVisibility();
        }

        void ShowHoverView(VisualElement view)
        {
            _hoverActive = true;
            _hoverColumn.style.display = DisplayStyle.Flex;
            HideAll(_hoverViews);
            view.style.display = DisplayStyle.Flex;
            UpdatePanelVisibility();
        }

        // === selection =====================================================
        // Pass the full selected list; the panel decides internally whether
        // to clear, show the single-item detail, or fall back to counts.
        // segmentCount/texture-name parameters only matter in the
        // exactly-one-selected case and are still placeholders.

        public void SetSelection(List<Vertex> vertices, List<Segment> segments,
            string leftTextureName = null, string rightTextureName = null)
        {
            int vertexCount = vertices?.Count ?? 0;
            int segmentCount = segments?.Count ?? 0;
            int total = vertexCount + segmentCount;

            if (total == 0) { ClearSelection(); return; }

            if (total == 1)
            {
                if (vertexCount == 1)
                {
                    ShowSelectionView(_selVertex);
                    _selVertex.Bind(vertices[0]);
                }
                else
                {
                    ShowSelectionView(_selSegment);
                    _selSegment.Bind(segments[0], leftTextureName, rightTextureName);
                }
                return;
            }

            ShowSelectionView(_selSummary);
            _selSummary.SetPair("vertices", vertexCount, "segments", segmentCount);
        }

        public void SetSelection(List<Region> regions, int segmentCount = 0,
            string floorTextureName = null, string ceilingTextureName = null)
        {
            int count = regions?.Count ?? 0;
            if (count == 0) { ClearSelection(); return; }

            if (count == 1)
            {
                ShowSelectionView(_selRegion);
                _selRegion.Bind(regions[0], segmentCount, floorTextureName, ceilingTextureName);
                return;
            }

            ShowSelectionView(_selSummary);
            _selSummary.SetSingle("regions", count);
        }

        public void SetSelection(List<MapObject> objects, string textureName = null)
        {
            int count = objects?.Count ?? 0;
            if (count == 0) { ClearSelection(); return; }

            if (count == 1)
            {
                ShowSelectionView(_selObject);
                _selObject.Bind(objects[0], textureName);
                return;
            }

            ShowSelectionView(_selSummary);
            _selSummary.SetSingle("objects", count);
        }

        public void SetSelection(List<Way> ways)
        {
            int count = ways?.Count ?? 0;
            if (count == 0) { ClearSelection(); return; }

            if (count == 1)
            {
                ShowSelectionView(_selWay);
                _selWay.Bind(ways[0]);
                return;
            }

            int vertexCount = 0;
            foreach (var w in ways)
                vertexCount += w.Positions.Count;

            ShowSelectionView(_selSummary);
            _selSummary.SetPair("ways", count, "vertices", vertexCount);
        }

        /// <summary>Call when nothing is selected - hides the selection column.</summary>
        public void ClearSelection()
        {
            _selectionActive = false;
            _selectionColumn.style.display = DisplayStyle.None;
            HideAll(_selectionViews);
            UpdatePanelVisibility();
        }

        /// <summary>Convenience for resetting the whole panel at once (e.g. tool deactivated, mouse left the viewport).</summary>
        public void ClearAll()
        {
            ClearHover();
            ClearSelection();
        }

        void ShowSelectionView(VisualElement view)
        {
            _selectionActive = true;
            _selectionColumn.style.display = DisplayStyle.Flex;
            HideAll(_selectionViews);
            view.style.display = DisplayStyle.Flex;
            UpdatePanelVisibility();
        }

        // === visibility ====================================================

        /// <summary>
        /// With nothing hovered or selected, the panel used to stay on
        /// screen as an empty padded/bordered box. Hide the whole element
        /// in that case instead of just its (already-empty) columns.
        /// Also toggles the selection column's separator line, which should
        /// only show when there's a hover column next to it to separate
        /// from.
        /// </summary>
        void UpdatePanelVisibility()
        {
            style.display = (_hoverActive || _selectionActive) ? DisplayStyle.Flex : DisplayStyle.None;

            if (_hoverActive)
                _selectionColumn.AddToClassList("info-panel__selection--separated");
            else
                _selectionColumn.RemoveFromClassList("info-panel__selection--separated");
        }

        static void HideAll(VisualElement[] views)
        {
            foreach (var view in views)
                view.style.display = DisplayStyle.None;
        }

        // === style sheet loading ==========================================

        /// <summary>
        /// AddToClassList only attaches class names - without a StyleSheet
        /// actually loaded into this element's styleSheets, none of the USS
        /// rules (including position/layout) take effect. Loads
        /// InfoPanel.uss from the same folder as this script, so nothing
        /// needs to be wired manually by the host tool.
        /// </summary>
        void LoadStyleSheet([CallerFilePath] string sourceFilePath = "")
        {
            var directory = Path.GetDirectoryName(sourceFilePath);
            if (string.IsNullOrEmpty(directory))
                return;

            var ussPath = Path.Combine(directory, "InfoPanel.uss").Replace('\\', '/');
            var dataPath = Application.dataPath;
            if (!ussPath.StartsWith(dataPath))
            {
                Debug.LogWarning($"InfoPanel: script path '{ussPath}' is outside Assets/, cannot resolve stylesheet automatically.");
                return;
            }

            var assetPath = "Assets" + ussPath.Substring(dataPath.Length);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"InfoPanel: could not find stylesheet at '{assetPath}'. Panel will render unstyled.");
        }
    }
}
