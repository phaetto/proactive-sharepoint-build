namespace ProActive.SharePoint.Build.Services.Contexts
{
    using ProActive.SharePoint.Build.Services.Contracts;

    public readonly struct ApplicationLoadContext
    {
        public ApplicationLoadContext(
            string uniqueBuildString,
            in SharePointProduct sharePointProduct,
            in SharePointWebPart[] sharePointWebParts
        )
        {
            SharePointProduct = sharePointProduct;
            SharePointWebParts = sharePointWebParts;
            UniqueBuildString = uniqueBuildString;
        }

        public SharePointWebPart[] SharePointWebParts { get; }
        public SharePointProduct SharePointProduct { get; }
        public string UniqueBuildString { get; }
    }
}
