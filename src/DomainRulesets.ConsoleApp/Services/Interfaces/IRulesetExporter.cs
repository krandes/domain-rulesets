using DomainRulesets.ConsoleApp.Models;

namespace DomainRulesets.ConsoleApp.Services.Interfaces;

public interface IRulesetExporter
{
    string FormatName { get; }
    
    void Export(IReadOnlyCollection<Ruleset> rulesets, string outputDirectory);
}