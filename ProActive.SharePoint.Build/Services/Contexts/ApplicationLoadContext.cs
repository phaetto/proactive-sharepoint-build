namespace ProActive.SharePoint.Build.Services.Contexts
{
    using ProActive.SharePoint.Build.Services.Contracts;

    public readonly struct ApplicationLoadContext
    {
        public ApplicationLoadContext(
            in string uniqueBuildString,
            in SharePointProduct sharePointProduct,
            in SharePointWebPart[] sharePointWebParts,
            in SharePointApplicationCustomizer[] sharePointApplicationCustomizers,
            in SharePointLibrary[] sharePointLibraries
        )
        {
            SharePointProduct = sharePointProduct;
            SharePointWebParts = sharePointWebParts;
            SharePointApplicationCustomizers = sharePointApplicationCustomizers;
            SharePointLibraries = sharePointLibraries;
            UniqueBuildString = uniqueBuildString;
        }

        public SharePointWebPart[] SharePointWebParts { get; }
        public SharePointApplicationCustomizer[] SharePointApplicationCustomizers { get; }
        public SharePointLibrary[] SharePointLibraries { get; }
        public SharePointProduct SharePointProduct { get; }
        public string UniqueBuildString { get; }
    }
}
