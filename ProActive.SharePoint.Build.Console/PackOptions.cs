namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;

    [Verb("pack", HelpText = "Creates an spfx file from folders in disc.")]
    public class PackOptions
    {
        [Option('s', "source-folder", Required = true, HelpText = "Sets the source folder of web resource files (html, js, css)")]
        public string SourceFolder { get; set; }

        [Option('f', "spfx-folder", Required = true, HelpText = "Sets folder that the spfx zip package will be generated")]
        public string SpfxFolder { get; set; }

        [Option('d', "debug-folder", Required = false, HelpText = "Sets the debug folder that the spfx component will be generated. Defaults to ./dist/debug.")]
        public string DebugFolder { get; set; }

        [Option('l', "templates-folder", Required = false, HelpText = "Sets the template folder that will get the base files for generation. Defaults to ./Templates/Creation.")]
        public string TemplatesFolder { get; set; }
    }
}
