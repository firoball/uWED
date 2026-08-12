namespace Editor.UI.Inspector
{
    /// <summary>
    /// Scene-wide totals, always shown regardless of hover/selection state.
    /// Plain struct of counts rather than a dictionary - add a field here
    /// (and one line in InfoPanel.SetStats) when a new count is needed, e.g.
    /// total textures.
    /// </summary>
    public struct InfoPanelStats
    {
        public int TotalObjects;
        public int TotalVertices;
        public int TotalSegments;
        public int TotalRegions;
        public int TotalWays;
    }
}
