using DomainRulesets.ConsoleApp.Constants;
using DomainRulesets.ConsoleApp.Enums;
using DomainRulesets.ConsoleApp.Models;
using DomainRulesets.ConsoleApp.Services.Interfaces;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using V2Ray.Core.App.Router.Routercommon;

namespace DomainRulesets.ConsoleApp.Services;

public class V2RayRulesetExporter(ILogger<V2RayRulesetExporter> logger) : IRulesetExporter
{
    private const string OutputFileName = "geosite.dat";

    public string FormatName => ExportFormats.V2Ray;

    public void Export(IReadOnlyCollection<Ruleset> rulesets, string outputDirectory)
    {
        var outputPath = Path.Combine(outputDirectory, OutputFileName);

        var v2RayGeoSiteList = MapToV2RayGeoSiteList(rulesets);

        SaveToFile(v2RayGeoSiteList, outputPath);

        logger.LogInformation(
            "Successfully exported V2Ray rulesets to '{OutputDirectory}'",
            outputDirectory);
    }

    private static GeoSiteList MapToV2RayGeoSiteList(IEnumerable<Ruleset> rulesets)
    {
        var v2RayGeoSiteList = new GeoSiteList();

        v2RayGeoSiteList.Entry.AddRange(rulesets.Select(MapToV2RayGeoSite));

        return v2RayGeoSiteList;
    }

    private static GeoSite MapToV2RayGeoSite(Ruleset ruleset)
    {
        var v2RayGeoSite = new GeoSite
        {
            CountryCode = ruleset.Name.ToUpper()
        };

        foreach (var rule in ruleset.Rules)
            v2RayGeoSite.Domain.Add(MapToV2RayDomain(rule));

        return v2RayGeoSite;
    }

    private static Domain MapToV2RayDomain(Rule rule)
    {
        var v2RayDomain = new Domain
        {
            Value = rule.Value,
            Type = MapToV2RayRuleType(rule.Type)
        };

        if (rule.Options is not null)
        {
            foreach (var attribute in rule.Options.Attributes)
                v2RayDomain.Attribute.Add(MapToV2RayAttribute(attribute));
        }

        return v2RayDomain;
    }

    private static Domain.Types.Type MapToV2RayRuleType(RuleType ruleType) => ruleType switch
    {
        RuleType.Full => Domain.Types.Type.Full,
        RuleType.Suffix => Domain.Types.Type.RootDomain,
        RuleType.Keyword => Domain.Types.Type.Plain,
        RuleType.Regexp => Domain.Types.Type.Regex,
        _ => throw new ArgumentOutOfRangeException(nameof(ruleType), $"Unsupported rule type: '{ruleType}'.")
    };

    private static Domain.Types.Attribute MapToV2RayAttribute(string attribute) =>
        new()
        {
            Key = attribute.TrimStart('@').ToLower(),
            BoolValue = true
        };

    private void SaveToFile(GeoSiteList v2RayGeoSiteList, string outputPath)
    {
        try
        {
            using var outputStream = File.Create(outputPath);

            v2RayGeoSiteList.WriteTo(outputStream);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to export V2Ray rulesets: 'geosite.dat' -> '{OutputPath}'",
                outputPath);

            throw;
        }
    }
}