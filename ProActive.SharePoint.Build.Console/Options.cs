namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;

    public class Options
    {
        [Option('s', "source-folder", Required = true, HelpText = "Sets the source folder of a published Blazor application")]
        public string SourceFolder { get; set; }

        [Option('f', "spfx-folder", Required = true, HelpText = "Sets folder that the spfx zip package will be generated")]
        public string SpfxFolder { get; set; }

        [Option('d', "debug-folder", Required = false, HelpText = "Sets the debug folder that the spfx component will be generated. Defaults to ./dist/debug.")]
        public string DebugFolder { get; set; }

        [Option('l', "templates-folder", Required = false, HelpText = "Sets the template folder that will get the base files for generation. Defaults to ./Templates/Creation.")]
        public string TemplatesFolder { get; set; }
    }
}
