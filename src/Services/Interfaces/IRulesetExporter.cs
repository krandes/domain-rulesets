using DomainRulesets.Models;

namespace DomainRulesets.Services.Interfaces;

public interface IRulesetExporter
{
    string FormatName { get; }
    
    void Export(IReadOnlyCollection<Ruleset> rulesets, string outputDirectory);
}