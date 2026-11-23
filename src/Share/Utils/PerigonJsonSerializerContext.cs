using Share.Models;
using System.Text.Json.Serialization;

namespace Share.Utils;

/// <summary>
/// JSON metadata used by the CLI's module package formats.
/// Keeping these types in a source-generated context avoids reflection-based
/// serialization when the command-line tool is published with Native AOT.
/// </summary>
[JsonSerializable(typeof(PackageMetadata))]
[JsonSerializable(typeof(List<PackageMetadata>))]
public partial class PerigonJsonSerializerContext : JsonSerializerContext
{
    public static PerigonJsonSerializerContext ModuleMetadata { get; } =
        new(new JsonSerializerOptions(ConstVal.DefaultJsonSerializerOptions));
}
