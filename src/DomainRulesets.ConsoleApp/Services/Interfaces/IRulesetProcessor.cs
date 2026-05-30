namespace DomainRulesets.ConsoleApp.Services.Interfaces;

public interface IRulesetProcessor
{
    public void Execute(string[] formats, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory);
}