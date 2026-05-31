using DomainRulesets.Enums;
using DomainRulesets.Exceptions;

namespace DomainRulesets.Models;

public sealed record Rule
{
    public RuleType Type { get; private init; }

    public string Value { get; private init; } = null!;
    
    public RuleOptions? Options { get; private init; }
    
    public Rule(RuleType type, string value, RuleOptions? options)
    {
        Type = type;
        Value = value;
        Options = options;
        
        Validate();
    }

    // ReSharper disable once UnusedMember.Local
    public Rule()
    {
    }
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value))
            throw new ValidationException("Rule value is required.");
    }
}