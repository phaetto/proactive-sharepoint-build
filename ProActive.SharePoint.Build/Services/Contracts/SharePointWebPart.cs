using System;

namespace ProActive.SharePoint.Build.Services.Contracts
{
    public sealed class SharePointWebPart
    {
        public string GuidId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string EntryPointFileName { get; set; }
    }
}
