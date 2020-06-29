namespace ProActive.SharePoint.Build.Services.Contracts
{
    public interface ISharePointEntryData
    {
        string Description { get; set; }
        string EntryPointFileName { get; set; }
        string GuidId { get; set; }
        string Title { get; set; }
        string Version { get; set; }
    }
}