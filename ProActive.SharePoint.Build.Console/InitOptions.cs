namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;
    using ProActive.SharePoint.Build.Services.Contracts;

    [Verb("init", HelpText = "Generates a the necessare product files that can create an SPFx cmponent.")]
    public class InitOptions
    {
        [Option('n', "name", Required = true, HelpText = "Sets the product name that is visible in app site collection.")]
        public string ProductName { get; set; }

        [Option('t', "tenant-wide-installation", Required = false, HelpText = "Sets the configuration flag that the product can be installed tenant wide.")]
        public bool IsTenantWideInstallation { get; set; }

        // SPFx kind option
        [Option('w', "webpart", Required = true, HelpText = "Creates a webpart configuration.", SetName = nameof(ClientSideType.WebPart))]
        public bool IsWebPart { get; set; }

        [Option('a', "application-customizer", Required = true, HelpText = "Creates an application customizer configuration.", SetName = nameof(ClientSideType.ApplicationCustomizer))]
        public bool IsApplicationCustomizer { get; set; }

        [Option('l', "library", Required = true, HelpText = "Creates an library configuration.", SetName = nameof(ClientSideType.Library))]
        public bool IsLibrary { get; set; }
    }
}
