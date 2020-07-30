namespace ProActive.SharePoint.Build.Services
{
    using ProActive.SharePoint.Build.Services.Contexts;
    using ProActive.SharePoint.Build.Services.Contracts;
    using ProActive.SharePoint.Build.Services.Extensions;
    using ProActive.SharePoint.Build.Services.Strings;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

            // TODO: do not make all the parts to be required, be more flexible
            Guard.ForNull(webPartProductSpec.SharePointProduct, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointProduct)}.");
            Guard.ForNull(webPartProductSpec.SharePointWebParts, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointWebParts)}.");
            Guard.ForNull(webPartProductSpec.SharePointApplicationCustomizers, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointApplicationCustomizers)}.");
            Guard.ForNull(webPartProductSpec.SharePointLibraries, $"Json document must contain an entry to {nameof(webPartProductSpec.SharePointLibraries)}.");
            
            Guard.ForNull(webPartProductSpec.SharePointProduct.GuidId, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.GuidId)} cannot be empty.");
            Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointProduct.Name, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.Name)} cannot be empty.");
            Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointProduct.Version, $"{nameof(webPartProductSpec.SharePointProduct)}::{nameof(SharePointProduct.Version)} cannot be empty.");
            
            webPartProductSpec.SharePointWebParts.ForEach((x, i) =>
            {
                Guard.ForNull(webPartProductSpec.SharePointWebParts[i].GuidId, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.GuidId)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Title, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Title)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Version, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Version)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].EntryPointFileName, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.EntryPointFileName)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointWebParts[i].Description, $"Item {i}: {nameof(webPartProductSpec.SharePointWebParts)}::{nameof(SharePointWebPart.Description)} cannot be empty.");
            });
            webPartProductSpec.SharePointApplicationCustomizers.ForEach((x, i) =>
            {
                Guard.ForNull(webPartProductSpec.SharePointApplicationCustomizers[i].GuidId, $"Item {i}: {nameof(webPartProductSpec.SharePointApplicationCustomizers)}::{nameof(SharePointApplicationCustomizer.GuidId)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointApplicationCustomizers[i].Title, $"Item {i}: {nameof(webPartProductSpec.SharePointApplicationCustomizers)}::{nameof(SharePointApplicationCustomizer.Title)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointApplicationCustomizers[i].Version, $"Item {i}: {nameof(webPartProductSpec.SharePointApplicationCustomizers)}::{nameof(SharePointApplicationCustomizer.Version)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointApplicationCustomizers[i].EntryPointFileName, $"Item {i}: {nameof(webPartProductSpec.SharePointApplicationCustomizers)}::{nameof(SharePointApplicationCustomizer.EntryPointFileName)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointApplicationCustomizers[i].Description, $"Item {i}: {nameof(webPartProductSpec.SharePointApplicationCustomizers)}::{nameof(SharePointApplicationCustomizer.Description)} cannot be empty.");
            });
            webPartProductSpec.SharePointLibraries.ForEach((x, i) =>
            {
                Guard.ForNull(webPartProductSpec.SharePointLibraries[i].GuidId, $"Item {i}: {nameof(webPartProductSpec.SharePointLibraries)}::{nameof(SharePointLibrary.GuidId)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointLibraries[i].Title, $"Item {i}: {nameof(webPartProductSpec.SharePointLibraries)}::{nameof(SharePointLibrary.Title)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointLibraries[i].Version, $"Item {i}: {nameof(webPartProductSpec.SharePointLibraries)}::{nameof(SharePointLibrary.Version)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointLibraries[i].EntryPointFileName, $"Item {i}: {nameof(webPartProductSpec.SharePointLibraries)}::{nameof(SharePointLibrary.EntryPointFileName)} cannot be empty.");
                Guard.ForNullOrWhiteSpace(webPartProductSpec.SharePointLibraries[i].Description, $"Item {i}: {nameof(webPartProductSpec.SharePointLibraries)}::{nameof(SharePointLibrary.Description)} cannot be empty.");
            });

            // TODO: Validation
            // - WebPart have the same GUID as the product, makes it fail
            // - Version format check

            // Defaults

            webPartProductSpec.SharePointWebParts.ForEach((x, i) =>
            {
                // TODO: write it in a better way
                if ((x.Dependencies?.All(y => y.Name != "@microsoft/sp-webpart-base")) ?? true)
                {
                    var list = x.Dependencies?.ToList() ?? new List<SharePointDependency>();
                    list.Insert(0, new SharePointDependency
                    {
                        Name = "@microsoft/sp-webpart-base",
                        Id = Guid.Parse("974a7777-0990-4136-8fa6-95d80114c2e0"),
                        Version = "1.9.1"
                    });

                    x.Dependencies = list.ToArray();
                }
            });

            webPartProductSpec.SharePointApplicationCustomizers.ForEach((x, i) =>
            {
                // TODO: write it in a better way
                if ((x.Dependencies?.All(y => y.Name != "@microsoft/sp-application-base")) ?? true)
                {
                    var list = x.Dependencies?.ToList() ?? new List<SharePointDependency>();
                    list.Insert(0, new SharePointDependency
                    {
                        Name = "@microsoft/sp-application-base",
                        Id = Guid.Parse("4df9bb86-ab0a-4aab-ab5f-48bf167048fb"),
                        Version = "1.10.0"
                    });

                    x.Dependencies = list.ToArray();
                }
            });

            webPartProductSpec.SharePointProduct.TenantWideInstallation = webPartProductSpec.SharePointLibraries.Length > 0;

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
                 webPartProductSpec.SharePointWebParts,
                 webPartProductSpec.SharePointApplicationCustomizers,
                 webPartProductSpec.SharePointLibraries
            );
        }
    }
}
