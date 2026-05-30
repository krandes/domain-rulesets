using System.CommandLine;
using DomainRulesets.ConsoleApp.Constants;
using DomainRulesets.ConsoleApp.Exceptions;
using DomainRulesets.ConsoleApp.Services;
using DomainRulesets.ConsoleApp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();

    builder.Services.AddSingleton<IRulesetImporter, YamlRulesetImporter>();
    builder.Services.AddSingleton<IRulesetExporter, V2RayRulesetExporter>();
    builder.Services.AddSingleton<IRulesetExporter, ShadowrocketRulesetExporter>();
    builder.Services.AddSingleton<IRulesetProcessor, RulesetProcessor>();

    using var host = builder.Build();

    var services = host.Services;

    var formatsOption = new Option<string[]>(name: "--formats", "-f")
    {
        Description = "Target formats (space-separated)",
        DefaultValueFactory = _ => ExportFormats.All.ToArray(),
        AllowMultipleArgumentsPerToken = true
    };

    var inputDirectoryOption = new Option<DirectoryInfo>(name: "--input-dir", "-i")
    {
        Description = "Directory containing input YAML rulesets",
        DefaultValueFactory = _ => new DirectoryInfo(path: "./rulesets"),
    };

    var outputDirectoryOption = new Option<DirectoryInfo>(name: "--output-dir", "-o")
    {
        Description = "Directory for the exported rulesets",
        DefaultValueFactory = _ => new DirectoryInfo(path: "./release"),
    };

    var rootCommand = new RootCommand(
        description: $"Domain routing rulesets exporter for various formats: {string.Join(", ", ExportFormats.All)}")
    {
        formatsOption,
        inputDirectoryOption,
        outputDirectoryOption
    };

    rootCommand.SetAction(parseResult =>
    {
        var rulesetProcessor = services.GetRequiredService<IRulesetProcessor>();

        rulesetProcessor.Execute(
            formats: parseResult.GetValue(formatsOption)!,
            inputDirectory: parseResult.GetValue(inputDirectoryOption)!,
            outputDirectory: parseResult.GetValue(outputDirectoryOption)!
        );
    });

    return await rootCommand.Parse(args).InvokeAsync();
}
catch (ValidationException exception)
{
    Log.Error(exception, "Validation failed");
    
    return 1;
}
catch (Exception exception)
{
    Log.Error(exception, "Application terminated unexpectedly");
    
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}