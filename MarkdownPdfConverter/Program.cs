using System;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace MarkdownPdfConverter
{
    class Program
    {
        public class Options
        {
            [Option('i', "inputFile", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('o', "outputFile", Required = false, HelpText = "Output file to be generated.")]
            public string OutputFile { get; set; }

            [Option('l', "licenseKeyForIronSoftware", Required = false, HelpText = "License key for Iron Software.", Default = "")]
            public string licenseKeyForIronSoftware { get; set; }

        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => TransformFile(opts.InputFile, opts.OutputFile, opts.licenseKeyForIronSoftware))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void TransformFile(string inputFile, string outputFile, string licenseKey)
        {
            AnsiConsole.Status()
                .Start("Processing your file...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);


                    if (string.IsNullOrEmpty(outputFile))
                    {
                        AnsiConsole.MarkupLine("[bold blue]Setting output file![/]");
                        outputFile = $"{System.IO.Path.GetDirectoryName(inputFile)}\\{System.IO.Path.GetFileNameWithoutExtension(inputFile)}.pdf";
                    }

                    if (string.IsNullOrEmpty(licenseKey))
                    {
                        AnsiConsole.MarkupLine("[bold blue]Setting IronPdf License Key![/]");
                        var builder = new ConfigurationBuilder()
                            .AddUserSecrets<Program>();

                        var configuration = builder.Build();
                        licenseKey = configuration["IronPdf:LicenseKey"];
                    }
                    AnsiConsole.MarkupLine("[bold blue]Validating License Key![/]");
                    if (!License.IsValidLicense(licenseKey))
                    {
                        AnsiConsole.MarkupLine("[bold red]License Key Invalid![/]");
                        return;
                    }
                    AnsiConsole.MarkupLine("[bold green]License Key Validated![/]");
                    License.LicenseKey = licenseKey;
                    Installation.EnableWebSecurity = true;
                   
                    AnsiConsole.MarkupLine("[bold blue]Reading Markup File[/]");
                    string markdown = File.ReadAllText(inputFile);
                    string html = Markdig.Markdown.ToHtml(markdown);


                    AnsiConsole.MarkupLine("[bold blue]Instantiating Renderer![/]");
                    var Renderer = new IronPdf.ChromePdfRenderer();
                    AnsiConsole.MarkupLine("[bold blue]Rendering to PDF![/]");
                    var Doc = Renderer.RenderHtmlAsPdf(html);

                    AnsiConsole.MarkupLine("[bold blue]Saving PDF![/]");
                    Doc.SaveAs(outputFile);
                    AnsiConsole.MarkupLine("[bold green]Finished!![/]");

                });
        }

        static void HandleParseError(IEnumerable<Error> errs)
            {
                // Handle errors here
                Console.WriteLine("Error parsing arguments");
            }
        }
    }
