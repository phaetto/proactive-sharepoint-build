namespace ProActive.SharePoint.Build.Console.Services
{
    using OfficeDevPnP.Core;
    using OfficeDevPnP.Core.ALM;
    using OfficeDevPnP.Core.Enums;

    public sealed class UploadAndDeployAppService
    {
        private readonly string sspkgFilePath;
        private readonly string tenantName;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly bool skipFeatureDeployment;

        public UploadAndDeployAppService(
            string sspkgFilePath,
            string tenantName,
            string clientId,
            string clientSecret,
            bool skipFeatureDeployment)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.skipFeatureDeployment = skipFeatureDeployment;
            this.sspkgFilePath = sspkgFilePath;
            this.tenantName = tenantName;
        }

        public void Process()
        {
            var siteUrl = $"https://{tenantName}.sharepoint.com/";
            using (var authenticationManager = new AuthenticationManager())
            using (var clientContext = authenticationManager.GetAppOnlyAuthenticatedContext(siteUrl, clientId, clientSecret))
            {
                var appManager = new AppManager(clientContext);
                var addedApplicationMetatdata = appManager.Add(sspkgFilePath, true, AppCatalogScope.Tenant);
                appManager.Deploy(addedApplicationMetatdata, skipFeatureDeployment: skipFeatureDeployment);
            };
        }
    }
}
