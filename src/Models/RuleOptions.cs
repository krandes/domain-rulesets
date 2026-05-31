namespace DomainRulesets.Models;

public sealed record RuleOptions
{
    public List<string> Attributes { get; private init; } = [];

    public RuleOptions(List<string>? attributes)
    {
        Attributes = attributes ?? [];
    }
    
    // ReSharper disable once UnusedMember.Local
    public RuleOptions()
    {
    }
}