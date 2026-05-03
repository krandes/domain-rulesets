namespace DomainRulesets.ConsoleApp.Constants;

public static class ExportFormats
{
    public const string Shadowrocket = "shadowrocket";
    public const string V2Ray = "v2ray";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Shadowrocket,
        V2Ray
    };
}