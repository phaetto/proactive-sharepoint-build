namespace ProActive.SharePoint.Build.Console
{
    using CommandLine;

    [Verb("upload", HelpText = "Uploads an spfx file to a tenant using app client Id and secret.")]
    public class UploadOptions
    {
        [Option('f', "spfx-file", Required = true, HelpText = "The spfx file path.")]
        public string SpfxFilePath { get; set; }

        [Option('t', "tenantname", Required = true, HelpText = "The tenant name that the file will be uploaded.")]
        public string TenantName { get; set; }

        [Option('c', "clientid", Required = true, HelpText = "The client Id of the app that has access.")]
        public string ClientId { get; set; }

        [Option('s', "clientsecret", Required = true, HelpText = "The client secret of the app that has access.")]
        public string ClientSecret { get; set; }

        [Option('d', "skip-feature-deployment", Required = false, HelpText = "When passed, the deployment will be tenant wide if the components support it.")]
        public bool SkipFeatureDeployment { get; set; }
    }
}
