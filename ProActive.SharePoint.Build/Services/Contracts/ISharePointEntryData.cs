namespace ProActive.SharePoint.Build.Services.Contracts
{
    using System;

    public interface ISharePointEntryData
    {
        string Description { get; set; }
        string EntryPointFileName { get; set; }
        Guid GuidId { get; set; }
        string Title { get; set; }
        string Version { get; set; }
        SharePointDependency[] Dependencies { get; set; }
    }
}