namespace ProActive.SharePoint.Build.Services.Contracts
{
    using System;

    public sealed class SharePointLibrary : ISharePointEntryData
    {
        public Guid GuidId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string EntryPointFileName { get; set; }
        public SharePointDependency[] Dependencies { get; set; }
    }
}
