using DomainRulesets.ConsoleApp.Exceptions;

namespace DomainRulesets.ConsoleApp.Models;

public sealed record Ruleset
{
    public string Name { get; init; } = null!;

    public List<Rule> Rules { get; init; } = [];

    public List<string> Includes { get; init; } = [];

    public Ruleset(string name, List<Rule>? rules, List<string>? includes)
    {
        Name = name;
        Rules = rules ?? [];
        Includes = includes ?? [];

        Validate();
    }

    // ReSharper disable once UnusedMember.Local
    public Ruleset()
    {
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ValidationException("Ruleset name is required.");

        if (Rules.Count == 0 && Includes.Count == 0)
            throw new ValidationException($"Ruleset '{Name}' must contain at least one rule or one include.");

        var uniqueRuleValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in Rules)
        {
            try
            {
                rule.Validate();
            }
            catch (ValidationException exception)
            {
                throw new ValidationException(
                    $"Validation failed for ruleset '{Name}': {exception.Message}",
                    exception);
            }

            if (!uniqueRuleValues.Add(rule.Value))
            {
                throw new ValidationException(
                    $"Duplicate rule '{rule.Value}' in ruleset '{Name}'. Values must be unique.");
            }
        }
    }
}