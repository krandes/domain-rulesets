using DomainRulesets.ConsoleApp.Constants;
using DomainRulesets.ConsoleApp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DomainRulesets.ConsoleApp.Services;

public class RulesetProcessor(
    IRulesetImporter rulesetImporter,
    IEnumerable<IRulesetExporter> rulesetExporters,
    ILogger<RulesetProcessor> logger) : IRulesetProcessor
{
    public void Execute(string[] formats, DirectoryInfo inputDirectory, DirectoryInfo outputDirectory)
    {
        if (!inputDirectory.Exists)
        {
            logger.LogError("Input directory not found: '{InputDirectory}'", inputDirectory.FullName);

            throw new DirectoryNotFoundException($"Input directory not found: '{inputDirectory.FullName}'.");
        }

        if (!outputDirectory.Exists)
        {
            logger.LogWarning(
                "Output directory not found. Trying to create: '{OutputDirectory}'",
                outputDirectory.FullName);

            outputDirectory.Create();
        }

        var rulesets = rulesetImporter.Import(inputDirectory);

        if (rulesets.Count == 0)
        {
            logger.LogWarning("No rulesets found to process");

            return;
        }

        foreach (var format in formats)
        {
            if (!ExportFormats.All.Contains(format))
                throw new ArgumentException($"Unsupported export format: {format}.");

            var rulesetExporter = rulesetExporters
                .Single(e => e.FormatName.Equals(format, StringComparison.OrdinalIgnoreCase));

            var innerOutputDirectory = new DirectoryInfo(Path.Combine(outputDirectory.FullName, format.ToLower()));

            if (!innerOutputDirectory.Exists)
                innerOutputDirectory.Create();

            rulesetExporter.Export(rulesets, innerOutputDirectory.FullName);
        }
    }
}