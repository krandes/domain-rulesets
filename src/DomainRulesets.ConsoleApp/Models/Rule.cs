using System.ComponentModel.DataAnnotations;
using DomainRulesets.ConsoleApp.Enums;

namespace DomainRulesets.ConsoleApp.Models;

public sealed record Rule
{
    public RuleType Type { get; init; }

    public string Value { get; init; } = null!;
    
    public RuleOptions? Options { get; init; }
    
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