namespace ProActive.SharePoint.Build.Services.Contracts
{
    using System;

    public sealed class SharePointProduct
    {
        public Guid GuidId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool TenantWideInstallation { get; set; }
    }
}
