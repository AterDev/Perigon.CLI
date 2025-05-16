namespace Share.Helper;
public class ProtobufHelper
{
    public static Dictionary<string, string> TypeMap = new()
    {
        ["decimal"] = "double",
        ["double"] = "double",
        ["float"] = "float",
        ["uint"] = "fixed32",
        ["ulong"] = "fixed64",
        ["int"] = "sfixed32",
        ["long"] = "sfixed64",
        ["bool"] = "bool",
        ["string"] = "string",
        ["Guid"] = "string",
        ["ByteString"] = "bytes",
        ["DateTimeOffset"] = "google.protobuf.Timestamp",
        ["DateTime"] = "google.protobuf.Timestamp",
        ["DateOnly"] = "google.protobuf.Timestamp",
        ["TimeOnly"] = "google.protobuf.Timestamp",
        ["TimeSpan"] = "google.protobuf.Duration",
        ["bool?"] = "google.protobuf.BoolValue",
        ["double?"] = "google.protobuf.DoubleValue",
        ["float?"] = "google.protobuf.FloatValue",
        ["int?"] = "google.protobuf.Int32Value",
        ["long?"] = "google.protobuf.Int64Value",
        ["uint?"] = "google.protobuf.UInt32Value",
        ["ulong?"] = "google.protobuf.UInt64Value",
        ["string?"] = "google.protobuf.StringValue",
        ["ByteString?"] = "google.protobuf.BytesValue",
    };
    // 字典和列表
}
