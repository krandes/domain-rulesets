using DomainRulesets.ConsoleApp.Models;

namespace DomainRulesets.ConsoleApp.Services.Interfaces;

public interface IRulesetImporter
{
    IReadOnlyCollection<Ruleset> Import(DirectoryInfo inputDirectory);
}