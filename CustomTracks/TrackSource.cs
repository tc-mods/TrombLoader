namespace TrombLoader.CustomTracks;

public enum TrackSource
{
    /// <summary>
    /// A track loaded from normal TrombLoader search paths.
    /// Appears in the TrombLoader collection.
    /// </summary>
    TrombLoader,

    /// <summary>
    /// A track loaded from another folder via the CustomTrackLoader mechanism.
    /// Does not appear in the TrombLoader collection.
    /// </summary>
    Other,
}
