namespace DomainRulesets.Services.Interfaces;

public interface IRulesetProcessor
{
    public void Execute(string[] formats, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory);
}