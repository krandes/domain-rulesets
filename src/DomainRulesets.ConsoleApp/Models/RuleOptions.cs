namespace DomainRulesets.ConsoleApp.Models;

public sealed record RuleOptions
{
    public List<string> Attributes { get; init; } = [];

    public RuleOptions(List<string>? attributes)
    {
        Attributes = attributes ?? [];
    }
    
    // ReSharper disable once UnusedMember.Local
    public RuleOptions()
    {
    }
}