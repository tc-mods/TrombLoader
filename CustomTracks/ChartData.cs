using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

public class ChartData
{
    [JsonConverter(typeof(ChartCompatibility.IntOrFloatConverter), "savednotespacing")]
    [JsonRequired]
    public int savednotespacing;
    [JsonConverter(typeof(ChartCompatibility.IntOrFloatConverter), "timesig")]
    [JsonRequired]
    public int timesig;

    [JsonRequired] public float[][] notes;
    public List<Lyric> lyrics = [];
    public float[] note_color_start = { 1.0f, 0.21f, 0f };
    public float[] note_color_end = { 1.0f, 0.8f, 0.3f };
    public float[][] improv_zones = {};
    public float[][] bgdata = {};

    public SavedLevel ToSavedLevel(CustomTrackData data)
    {
        return new SavedLevel
        {
            savedleveldata = new List<float[]>(notes),
            bgdata = new List<float[]>(bgdata),
            improv_zones = new List<float[]>(improv_zones),
            endpoint = data.endpoint,
            lyricspos = lyrics.Select(lyric => new[] { lyric.bar, 0 }).ToList(),
            lyricstxt = lyrics.Select(lyric => lyric.text).ToList(),
            note_color_start = note_color_start,
            note_color_end = note_color_end,
            savednotespacing = savednotespacing,
            tempo = data.tempo,
            timesig = timesig
        };
    }
}
