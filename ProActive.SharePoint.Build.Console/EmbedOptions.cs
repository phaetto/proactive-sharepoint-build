namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;

    [Verb("embed", HelpText = "Creates an spfx file from direct source input.")]
    public class EmbedOptions
    {
        [Option('j', "js", Required = true, HelpText = "Sets the custom JavaScript for an spfx.")]
        public string JavaScriptContent { get; set; }
    }
}
