namespace ProActive.SharePoint.Build.Services
{
    using ProActive.SharePoint.Build.Services.Contexts;
    using ProActive.SharePoint.Build.Services.Contracts;
    using ProActive.SharePoint.Build.Services.Extensions;
    using ProActive.SharePoint.Build.Services.Strings;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    /// Prepares the environment for file processing
    /// </summary>
    public sealed class InitSpfxFolderStructureService
    {
        private readonly string sourceFolder;
        private readonly string spfxOutputFolder;

        public InitSpfxFolderStructureService(
            string sourceFolder,
            string spfxOutputFolder
        )
        {
            this.sourceFolder = sourceFolder;
            this.spfxOutputFolder = spfxOutputFolder;
        }

        public ApplicationLoadContext Process()
        {
            var specFile = Path.Combine(sourceFolder, Files.WebPartProduct);

            Guard.ForInvalidFile(specFile, $"The source folder must have a root file ${Files.WebPartProduct}.");

            var webPartProductSpec = JsonSerializer.Deserialize<WebPartProductSpecification>(File.ReadAllText(specFile));

            Guard.ForNull(webPartProductSpec.SharePointProduct, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointProduct)}.");
            Guard.ForNull(webPartProductSpec.SharePointWebParts, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointWebParts)}.");
            Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointProduct.GuidId, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.GuidId)} cannot be empty.");
            Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointProduct.Name, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.Name)} cannot be empty.");
            Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointProduct.Version, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.Version)} cannot be empty.");
            Guard.ForEmptyList(webPartProductSpec.SharePointWebParts, $"{nameof(webPartProductSpec.SharePointWebParts)} cannot be an empty list.");
            webPartProductSpec.SharePointWebParts.ForEach((x, i) =>
            {
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].GuidId, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.GuidId)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Title, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Title)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Version, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Version)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].EntryPointFileName, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.EntryPointFileName)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Description, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Description)} cannot be empty.");
            });

            // Remove the existing files in the folder
            var directoryInfo = new DirectoryInfo(spfxOutputFolder);

            if (directoryInfo.Exists)
            {
                foreach (var file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (var directory in directoryInfo.GetDirectories())
                {
                    directory.Delete(true);
                }
            }
            else
            {
                directoryInfo.Create();
            }

            Directory.CreateDirectory(Path.Combine(spfxOutputFolder, "ClientSideAssets"));

            return new ApplicationLoadContext(
                 TextManipulation.RandomString(30),
                 webPartProductSpec.SharePointProduct,
                 webPartProductSpec.SharePointWebParts
            );
        }
    }
}
