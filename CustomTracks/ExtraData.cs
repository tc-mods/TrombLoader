using Newtonsoft.Json.Linq;

namespace TrombLoader.CustomTracks;

public class ExtraData
{
    private Identifier _identifier;
    private JObject _contents;

    public ExtraData(Identifier identifier, JObject contents)
    {
        _identifier = identifier;
        _contents = contents;
    }

    public Identifier Identifier => _identifier;
    public JObject GetJsonObject() => _contents;
    public JToken this[string key] => _contents[key];
}
