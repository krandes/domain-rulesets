using DomainRulesets.Models;

namespace DomainRulesets.Services.Interfaces;

public interface IRulesetImporter
{
    IReadOnlyCollection<Ruleset> Import(DirectoryInfo inputDirectory);
}