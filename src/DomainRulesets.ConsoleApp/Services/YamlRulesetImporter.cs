using DomainRulesets.ConsoleApp.Converters;
using DomainRulesets.ConsoleApp.Exceptions;
using DomainRulesets.ConsoleApp.Models;
using DomainRulesets.ConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DomainRulesets.ConsoleApp.Services;

public class YamlRulesetImporter(ILogger<YamlRulesetImporter> logger) : IRulesetImporter
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new StringTrimmingConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    public IReadOnlyCollection<Ruleset> Import(DirectoryInfo inputDirectory)
    {
        var rawRulesets = new Dictionary<string, Ruleset>(StringComparer.OrdinalIgnoreCase);

        var files = inputDirectory
            .EnumerateFiles(searchPattern: "*.*", searchOption: SearchOption.AllDirectories)
            .Where(f => f.Extension is ".yaml" or ".yml");

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file.FullName);
                var ruleset = _deserializer.Deserialize<Ruleset>(content);

                ruleset.Validate();

                if (!rawRulesets.TryAdd(ruleset.Name, ruleset))
                    throw new ValidationException($"Ruleset '{ruleset.Name}' is already defined.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to parse ruleset file: '{FileName}'", file.Name);

                throw;
            }
        }

        return ExpandRulesets(rawRulesets);
    }

    private List<Ruleset> ExpandRulesets(IReadOnlyDictionary<string, Ruleset> rulesets)
    {
        var expandedRulesets = new List<Ruleset>();

        foreach (var name in rulesets.Keys)
            expandedRulesets.Add(MergeIncludedRulesets(name, rulesets, visitStack: []));

        return expandedRulesets;
    }

    private Ruleset MergeIncludedRulesets(
        string name,
        IReadOnlyDictionary<string, Ruleset> allRulesets,
        IReadOnlyCollection<string> visitStack)
    {
        if (visitStack.Contains(name))
        {
            var chain = string.Join(" -> ", visitStack) + $" -> {name}";

            logger.LogError("Circular include detected: '{Chain}'", chain);

            throw new ValidationException($"Circular dependency: '{chain}'.");
        }

        if (!allRulesets.TryGetValue(name, out var currentRuleset))
        {
            var context = visitStack.Count > 0 ? $" (referenced in '{visitStack.Last()}')" : "";

            logger.LogError("Ruleset '{Name}' not found {Context}", name, context);

            throw new ValidationException($"Ruleset '{name}' was not found in input directory.");
        }

        var uniqueRules = new Dictionary<string, Rule>();

        foreach (var rule in currentRuleset.Rules)
            uniqueRules[rule.Value.ToLower()] = rule;

        var newStack = new List<string>(visitStack)
        {
            name
        };

        foreach (var includeName in currentRuleset.Includes)
        {
            var includedRuleset = MergeIncludedRulesets(includeName, allRulesets, newStack);

            foreach (var rule in includedRuleset.Rules)
                uniqueRules[rule.Value.ToLower()] = rule;
        }

        return new Ruleset(name, rules: uniqueRules.Values.ToList(), includes: null);
    }
}