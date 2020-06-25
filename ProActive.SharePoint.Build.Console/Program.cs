namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;
    using ProActive.SharePoint.Build.Services;
    using ProActive.SharePoint.Build.Console.Extensions;
    using System;
    using System.IO;
    using System.IO.Compression;

    internal class Program
    {
        private const string Intro = "TODO: Intro";

        static void Main(string[] args)
        {
            /*
             * Input:
             *      Json file with SharePointProduct / SharePointWebPart spec
             *      Source folder (content)
             *      Output folder
             * */


            ConsoleExtensions.WriteLineWithColor(Intro, ConsoleColor.Yellow);
            ConsoleExtensions.WriteLineWithColor("\tfor SharePoint Online!", ConsoleColor.Cyan);
            ConsoleExtensions.WriteLineWithColor("\n\n\tWith <3 from ProActive", ConsoleColor.Cyan);
            ConsoleExtensions.WriteLineWithColor("\tContact: ama@proactive.dk\n\n", ConsoleColor.Cyan);

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    string sourceFolder = o.SourceFolder;
                    string targetFolder = o.TargetFolder ?? Path.GetFullPath(Path.Combine(".", "dist", "debug"));
                    string creationTemplatesFolder = string.IsNullOrWhiteSpace(o.TemplatesFolder)
                        ? Path.GetFullPath(Path.Combine(".", "Templates", "Creation"))
                        : Path.GetFullPath(Path.Combine(o.TemplatesFolder, "Templates", "Creation"));
                    string sspkgFolder = o.SpfxFolder ?? Path.GetFullPath(".");

                    // Initialize environment
                    var initSpfxFolderStructure = new InitSpfxFolderStructureService(sourceFolder, targetFolder);
                    var applicationLoadContext = initSpfxFolderStructure.Process();

                    Console.WriteLine("Creating spfx structure...");

                    var createSpfxFolderStructure = new CreateSpfxFolderStructureService(targetFolder, creationTemplatesFolder, applicationLoadContext);
                    createSpfxFolderStructure.Process();
                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);

                    Console.WriteLine("Creating artifacts...");

                    // TODO: move content to argument
                    var copyFilesToSpfxFolder = new CopyFilesToSpfxFolderService(sourceFolder, targetFolder, Path.GetFullPath(Path.Combine(".", "Content")),  applicationLoadContext);
                    copyFilesToSpfxFolder.Process();
                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);

                    Console.WriteLine("Creating zip archive...");

                    var sspkgFile = Path.GetFullPath(Path.Combine(sspkgFolder, $"{applicationLoadContext.SharePointProduct.Name}.sspkg"));
                    if (File.Exists(sspkgFile))
                    {
                        File.Delete(sspkgFile);
                    }
                    ZipFile.CreateFromDirectory(targetFolder, sspkgFile);

                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);
                })
                .WithNotParsed(errs =>
                {
                    // TODO: errs
                    ConsoleExtensions.WriteLineWithColor("Invalid arguments.", ConsoleColor.Yellow);
                });
        }
    }
}
