namespace uWED.Acknex.Runtime.Registry
{
    /// <summary>
    /// Optional performance hook for an IAssetStorageService backend that can batch a run of Create/Clone
    /// calls more cheaply than paying each one's per-operation cost individually (the Editor backend defers
    /// AssetDatabase's import/save to once per batch instead of once per call). A caller doing many such
    /// calls in a row (see TemplateRegistry.BeginBatch/EndBatch, used by TemplateResolver.ResolveAll) checks
    /// for this via an `is`/`as` pattern rather than every IAssetStorageService implementation being forced
    /// to provide a no-op for a concept that may not apply to it at all (e.g. a future Standalone backend
    /// doing its own direct file I/O has no equivalent cost to defer).
    /// </summary>
    public interface IBatchableAssetStorage
    {
        /// <summary>Starts a batch - operations between this and the matching EndBatch defer whatever
        /// per-operation cost this backend would otherwise pay on each one. Nesting-safe: only the
        /// outermost Begin/End pair actually starts/stops the batch.</summary>
        void BeginBatch();

        /// <summary>Ends a batch started by BeginBatch, flushing whatever was deferred.</summary>
        void EndBatch();
    }
}
