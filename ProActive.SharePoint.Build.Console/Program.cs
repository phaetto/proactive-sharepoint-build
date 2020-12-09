namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;
    using ProActive.SharePoint.Build.Services;
    using ProActive.SharePoint.Build.Console.Extensions;
    using System;
    using System.IO;
    using System.IO.Compression;
    using ProActive.SharePoint.Build.Services.Contracts;
    using System.Text.Json;

    internal class Program
    {
        private const string Intro = @"

  ███████ ██████  ███████ ██   ██     ██████  ██    ██ ██ ██      ██████  ███████ ██████  
  ██      ██   ██ ██       ██ ██      ██   ██ ██    ██ ██ ██      ██   ██ ██      ██   ██ 
  ███████ ██████  █████     ███       ██████  ██    ██ ██ ██      ██   ██ █████   ██████  
       ██ ██      ██       ██ ██      ██   ██ ██    ██ ██ ██      ██   ██ ██      ██   ██ 
  ███████ ██      ██      ██   ██     ██████   ██████  ██ ███████ ██████  ███████ ██   ██ 
                                                                                        
";
        private const string DefaultEntryPointFileName = "index.js";

        static void Main(string[] args)
        {
            ConsoleExtensions.WriteLineWithColor(Intro, ConsoleColor.Yellow);
            ConsoleExtensions.WriteLineWithColor("\tfor SharePoint Online!", ConsoleColor.Cyan);
            ConsoleExtensions.WriteLineWithColor("\n\n\tMade with <3 from ProActive - Contact: ama@proactive.dk\n\n", ConsoleColor.Cyan);

            var parser = new Parser(with => with.HelpWriter = Console.Out);
            var parserResult = parser.ParseArguments<InitOptions, PackOptions>(args);
            _ = parserResult
                .WithParsed<PackOptions>(o =>
                {
                    string sourceFolder = o.SourceFolder;
                    string debugFolder = o.DebugFolder ?? Path.GetFullPath(Path.Combine(".", "dist", "debug"));
                    string creationTemplatesFolder = string.IsNullOrWhiteSpace(o.TemplatesFolder)
                        ? Path.GetFullPath(Path.Combine(".", "Templates", "Creation"))
                        : Path.GetFullPath(o.TemplatesFolder);
                    string sspkgFolder = o.SpfxFolder ?? Path.GetFullPath(".");

                    // Initialize environment
                    var initSpfxFolderStructure = new InitSpfxFolderStructureService(sourceFolder, debugFolder);
                    var applicationLoadContext = initSpfxFolderStructure.Process();

                    Console.WriteLine("Creating spfx structure...");

                    var createSpfxFolderStructure = new CreateSpfxFolderStructureService(debugFolder, creationTemplatesFolder, applicationLoadContext);
                    createSpfxFolderStructure.Process();
                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);

                    Console.WriteLine("Creating artifacts...");

                    // TODO: move content to argument
                    var copyFilesToSpfxFolder = new CopyFilesToSpfxFolderService(sourceFolder, debugFolder, Path.GetFullPath(Path.Combine(".", "Content")), applicationLoadContext);
                    copyFilesToSpfxFolder.Process();
                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);

                    Console.WriteLine("Creating zip archive...");

                    var sspkgFile = Path.GetFullPath(Path.Combine(sspkgFolder, $"{applicationLoadContext.SharePointProduct.Name}.sspkg"));
                    if (File.Exists(sspkgFile))
                    {
                        File.Delete(sspkgFile);
                    }
                    ZipFile.CreateFromDirectory(debugFolder, sspkgFile);

                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);
                })
                //.WithParsed<EmbedOptions>(o =>
                //{
                //    // TODO: use direct input and create an spfx
                //})
                .WithParsed<InitOptions>(o =>
                {
                    if (Directory.Exists(o.ProductName))
                    {
                        ConsoleExtensions.WriteLineWithColor("Directory already exists.", ConsoleColor.Red);
                        return;
                    }

                    Directory.CreateDirectory(o.ProductName);

                    var webPartProductSpecification = new WebPartProductSpecification
                    {
                        SharePointProduct = new SharePointProduct
                        {
                            GuidId = Guid.NewGuid(),
                            Name = o.ProductName,
                            TenantWideInstallation = o.IsTenantWideInstallation,
                            Version = "0.0.0.1",
                        }
                    };

                    switch (o)
                    {
                        case { IsApplicationCustomizer: true }:
                            webPartProductSpecification.SharePointApplicationCustomizers = new[] {
                                new SharePointApplicationCustomizer {
                                    Title = $"'{o.ProductName}' application customizer",
                                    Description = $"'{o.ProductName}' application customizer",
                                    Version = "0.0.0.1",
                                    EntryPointFileName = DefaultEntryPointFileName,
                                    GuidId = Guid.NewGuid(),
                                    Dependencies = new SharePointDependency[0],
                                }
                            };
                            webPartProductSpecification.SharePointLibraries = new SharePointLibrary[0];
                            webPartProductSpecification.SharePointWebParts = new SharePointWebPart[0];
                            break;
                        case { IsWebPart: true }:
                            webPartProductSpecification.SharePointWebParts = new[] {
                                new SharePointWebPart {
                                    Title = $"'{o.ProductName}' webpart",
                                    Description = $"'{o.ProductName}' webpart",
                                    Version = "0.0.0.1",
                                    EntryPointFileName = DefaultEntryPointFileName,
                                    GuidId = Guid.NewGuid(),
                                    Dependencies = new SharePointDependency[0],
                                }
                            };
                            webPartProductSpecification.SharePointApplicationCustomizers = new SharePointApplicationCustomizer[0];
                            webPartProductSpecification.SharePointLibraries = new SharePointLibrary[0];
                            break;
                        case { IsLibrary: true }:
                            webPartProductSpecification.SharePointLibraries = new[] {
                                new SharePointLibrary {
                                    Title = $"'{o.ProductName}' library",
                                    Description = $"'{o.ProductName}' library",
                                    Version = "0.0.0.1",
                                    EntryPointFileName = DefaultEntryPointFileName,
                                    GuidId = Guid.NewGuid(),
                                    Dependencies = new SharePointDependency[0],
                                }
                            };
                            webPartProductSpecification.SharePointApplicationCustomizers = new SharePointApplicationCustomizer[0];
                            webPartProductSpecification.SharePointWebParts = new SharePointWebPart[0];
                            break;
                        default:
                            throw new InvalidOperationException("Internal error: Could not find an option");
                    }

                    var productFilePath = Path.Combine(o.ProductName, "product.json");
                    File.WriteAllText(productFilePath, JsonSerializer.Serialize(webPartProductSpecification, new JsonSerializerOptions { MaxDepth = 64, WriteIndented = true }));

                    var indexJsPath = Path.Combine(o.ProductName, "index.js");
                    File.WriteAllText(indexJsPath, $"console.log('[${o.ProductName}] component loaded!');");
                    
                    ConsoleExtensions.WriteLineWithColor($"A new SPFx application template created at: .{Path.DirectorySeparatorChar}{o.ProductName}", ConsoleColor.Green);
                })
                .WithNotParsed(errors =>
                {
                });
        }
    }
}
