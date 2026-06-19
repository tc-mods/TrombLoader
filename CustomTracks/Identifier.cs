using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

[TypeConverter(typeof(IdentifierTypeConverter))]
public class Identifier
{
    private static readonly Regex IdentifierRegex = new(@"^([a-zA-Z_-]+):([a-zA-Z0-9_-]+)$");

    public string Namespace { get; }
    public string Path { get; }

    public Identifier(string @namespace, string path)
    {
        Namespace = @namespace;
        Path = path;
    }

    [JsonConstructor]
    public Identifier(string identifier)
    {
        var m = IdentifierRegex.Match(identifier);
        if (!m.Success) throw new ArgumentException("Invalid identifier string: " + identifier);

        Namespace = m.Groups[1].Value;
        Path = m.Groups[2].Value;
    }

    public override string ToString() => $"{Namespace}:{Path}";

    public override int GetHashCode()
    {
        unchecked
        {
            return (Namespace.GetHashCode() * 397) ^ Path.GetHashCode();
        }
    }

    protected bool Equals(Identifier other)
    {
        return Namespace == other.Namespace && Path == other.Path;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Identifier)obj);
    }

    public static Identifier Parse(string identifier) => new(identifier);

    /// <summary>
    /// Needed to be able to use an <see cref="Identifier"/> as a dictionary key, when deserializing with Newtonsoft
    /// JSON.NET
    /// </summary>
    private sealed class IdentifierTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            sourceType == typeof(string);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            new Identifier((string)value);
    }
}
