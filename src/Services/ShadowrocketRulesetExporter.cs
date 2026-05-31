using DomainRulesets.Constants;
using DomainRulesets.Enums;
using DomainRulesets.Models;
using DomainRulesets.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DomainRulesets.Services;

internal sealed class ShadowrocketRulesetExporter(ILogger<ShadowrocketRulesetExporter> logger) : IRulesetExporter
{
    public string FormatName => ExportFormats.Shadowrocket;

    public void Export(IReadOnlyCollection<Ruleset> rulesets, string outputDirectory)
    {
        foreach (var ruleset in rulesets)
        {
            var outputPath = Path.Combine(outputDirectory, $"{ruleset.Name}.list");

            var lines = ruleset.Rules.Select(FormatRule).ToList();

            try
            {
                File.WriteAllLines(outputPath, lines);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to export Shadowrocket ruleset: '{RulesetName}' -> '{OutputRulesetPath}'",
                    ruleset.Name,
                    outputPath);

                throw;
            }
        }

        logger.LogInformation(
            "Successfully exported Shadowrocket rulesets to '{OutputDirectory}'",
            outputDirectory);
    }

    private static string FormatRule(Rule rule)
    {
        var shadowrocketRuleType = MapToShadowrocketRuleType(rule.Type);

        return $"{shadowrocketRuleType},{rule.Value}";
    }

    private static string MapToShadowrocketRuleType(RuleType ruleType) => ruleType switch
    {
        RuleType.Full => "DOMAIN",
        RuleType.Suffix => "DOMAIN-SUFFIX",
        RuleType.Keyword => "DOMAIN-KEYWORD",
        RuleType.Regexp => "URL-REGEX",
        _ => throw new ArgumentOutOfRangeException(nameof(ruleType), $"Unsupported rule type: '{ruleType}'.")
    };
}