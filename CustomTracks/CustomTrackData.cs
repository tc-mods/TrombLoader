using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

[JsonObject]
public class CustomTrackData
{
    [JsonRequired] public string trackRef;
    [JsonRequired] public string name;
    [JsonRequired] public string shortName;
    [JsonRequired] public string author;
    [JsonRequired] public string description;
    [JsonRequired] public float endpoint;
    [JsonRequired] public int year;
    [JsonRequired] public string genre;
    [JsonRequired] public int difficulty;
    [JsonRequired] public float tempo;
    public string backgroundMovement = "none";
}
