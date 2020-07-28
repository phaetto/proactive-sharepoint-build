namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;
    using ProActive.SharePoint.Build.Services;
    using ProActive.SharePoint.Build.Console.Extensions;
    using System;
    using System.IO;
    using System.IO.Compression;
    using ProActive.SharePoint.Build.Console.Services;

    internal class Program
    {
        private const string Intro = @"

  ███████ ██████  ███████ ██   ██     ██████  ██    ██ ██ ██      ██████  ███████ ██████  
  ██      ██   ██ ██       ██ ██      ██   ██ ██    ██ ██ ██      ██   ██ ██      ██   ██ 
  ███████ ██████  █████     ███       ██████  ██    ██ ██ ██      ██   ██ █████   ██████  
       ██ ██      ██       ██ ██      ██   ██ ██    ██ ██ ██      ██   ██ ██      ██   ██ 
  ███████ ██      ██      ██   ██     ██████   ██████  ██ ███████ ██████  ███████ ██   ██ 
                                                                                        
";

        static void Main(string[] args)
        {
            ConsoleExtensions.WriteLineWithColor(Intro, ConsoleColor.Yellow);
            ConsoleExtensions.WriteLineWithColor("\tfor SharePoint Online!", ConsoleColor.Cyan);
            ConsoleExtensions.WriteLineWithColor("\n\n\tMade with <3 from ProActive - Contact: ama@proactive.dk\n\n", ConsoleColor.Cyan);

            var parser = new Parser(with => with.HelpWriter = Console.Out);
            var parserResult = parser.ParseArguments<UploadOptions>(args);
            _ = parserResult
                .WithParsed(o =>
                {
                    Console.WriteLine("Uploading...");

                    var uploadAndDeployAppService = new UploadAndDeployAppService(o.SpfxFilePath, o.TenantName, o.ClientId, o.ClientSecret, o.SkipFeatureDeployment);
                    uploadAndDeployAppService.Process();

                    ConsoleExtensions.WriteLineWithColor("Done!", ConsoleColor.Green);
                })
                .WithNotParsed(errors =>
                {
                });
        }
    }
}
