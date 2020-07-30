using System.Text.Json.Serialization;

namespace ProActive.SharePoint.Build.Services.Contracts
{
    public class WebPartProductSpecification
    {
        public SharePointProduct SharePointProduct { get; set; }
        public SharePointWebPart[] SharePointWebParts { get; set; }
        public SharePointApplicationCustomizer[] SharePointApplicationCustomizers { get; set; }
        public SharePointLibrary[] SharePointLibraries { get; set; }
    }
}
