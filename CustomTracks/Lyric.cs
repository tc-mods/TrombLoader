using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

[JsonObject]
public class Lyric
{
    public string text { get; }
    public float bar { get; }

    [JsonConstructor]
    public Lyric(string text, float bar)
    {
        this.text = text;
        this.bar = bar;
    }
}
