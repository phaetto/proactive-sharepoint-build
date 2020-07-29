using System;

namespace ProActive.SharePoint.Build.Services.Contracts
{
    public sealed class SharePointDependency
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
