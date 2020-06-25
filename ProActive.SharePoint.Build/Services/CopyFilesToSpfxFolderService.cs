namespace ProActive.SharePoint.Build.Services
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text.Json;
    using System.Linq;
    using System.Text.RegularExpressions;
    using ProActive.SharePoint.Build.Services.Strings;
    using ProActive.SharePoint.Build.Services.Contracts;
    using ProActive.SharePoint.Build.Services.Contexts;

    public sealed class CopyFilesToSpfxFolderService
    {
        private readonly string sourceFolder;
        private readonly string spfxOutputFolder;
        private readonly string webAssemblyFolder;
        private readonly ApplicationLoadContext applicationLoadContext;

        public CopyFilesToSpfxFolderService(
            string sourceFolder,
            string spfxOutputFolder,
            string webAssemblyFolder,
            ApplicationLoadContext applicationLoadContext
        )
        {
            this.sourceFolder = sourceFolder;
            this.spfxOutputFolder = spfxOutputFolder;
            this.webAssemblyFolder = webAssemblyFolder;
            this.applicationLoadContext = applicationLoadContext;
        }

        public void Process()
        {
            // Get all source files
            var sourceFiles = Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(x => Path.HasExtension(x));

            // Find the product ID
            var appManifestXml = new XmlDocument();
            appManifestXml.Load($"{spfxOutputFolder}{Path.DirectorySeparatorChar}{Files.AppManifest}");
            var xmlnsManager = new XmlNamespaceManager(appManifestXml.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/2012/app/manifest");
            var productIDAttributeNode = appManifestXml.SelectSingleNode("/ns:App/@ProductID", xmlnsManager);
            var productID = Guid.Parse(productIDAttributeNode.Value);

            // Open the client-side-assets xml file
            var clientSideAssetsXml = new XmlDocument();
            var clientSideAssetsFileFullPath = $"{spfxOutputFolder}{Path.DirectorySeparatorChar}{Paths.ClientSideAssetsFile}";
            clientSideAssetsXml.Load(clientSideAssetsFileFullPath);
            xmlnsManager = new XmlNamespaceManager(clientSideAssetsXml.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/package/2006/relationships");
            var relationshipsNode = clientSideAssetsXml.SelectSingleNode("/ns:Relationships", xmlnsManager);

            // Clean up the nodes and files
            var relationshipNodes = clientSideAssetsXml.SelectNodes("/ns:Relationships/ns:Relationship[@Type='http://schemas.microsoft.com/sharepoint/2012/app/relationships/clientsideasset']", xmlnsManager);
            foreach (XmlElement xmlElement in relationshipNodes)
            {
                relationshipsNode.RemoveChild(xmlElement);
            }

            var spfxClientSideAssetsDirectoryFiles = Directory.EnumerateFiles(Path.Combine(spfxOutputFolder, "ClientSideAssets"), "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in spfxClientSideAssetsDirectoryFiles)
            {
                File.Delete(file);
            }

            var sourceFilesArray = sourceFiles.ToArray();
            
            // Process new files

            var count = 1;
            foreach (var file in sourceFilesArray)
            {
                CopyFileAndAddToXmlRel(relationshipsNode, productID, clientSideAssetsXml, count, sourceFolder, file);

                ++count;
            }

            ProcessCssFiles(sourceFolder, sourceFilesArray);

            //foreach (var attributeAndType in applicationLoadContext.SharePointWebPartAttributesAndTypes)
            //{
            //    // TODO: We should have the file entry before
            //    ProcessJsMainModuleFile(relationshipsNode, productID, clientSideAssetsXml, count, contentTemplateFilesFolder, Path.Combine(contentTemplateFilesFolder, "module.client.js"), attributeAndType);

            //    ++count;
            //}

            clientSideAssetsXml.PreserveWhitespace = true;
            clientSideAssetsXml.Save(clientSideAssetsFileFullPath);
        }

        private string CopyFileAndAddToXmlRel(XmlNode relationshipsNode, Guid productID, XmlDocument clientSideAssetsXml, int idCount, string rootDirectory, string file, string targetFilename = null)
        {
            if (string.IsNullOrWhiteSpace(targetFilename))
            {
                targetFilename = EscapeFile(Path.GetRelativePath(rootDirectory, file));
            }
            var newFilePath = Path.Combine(spfxOutputFolder, "ClientSideAssets", targetFilename);

            File.Copy(file, newFilePath, overwrite: true);

            // Add file to XML doc
            var newNode = clientSideAssetsXml.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
            newNode.SetAttribute("Type", "http://schemas.microsoft.com/sharepoint/2012/app/relationships/clientsideasset");
            newNode.SetAttribute("Target", $"/ClientSideAssets/{targetFilename}");
            //newNode.SetAttribute("Id", string.Format(Paths.IdFormatPattern, idCount));
            relationshipsNode.AppendChild(newNode);
            return newFilePath;
        }

        private void ProcessCssFiles(string rootDirectory, string[] sourceFiles)
        {
            foreach (var cssFile in sourceFiles.Where(x => Path.GetExtension(x) == ".css"))
            {
                var targetCssFilename = EscapeFile(Path.GetRelativePath(rootDirectory, cssFile));
                var targetCssFilePath = Path.Combine(spfxOutputFolder, "ClientSideAssets", targetCssFilename);
                var cssText = File.ReadAllText(targetCssFilePath);
                var matches = Regex.Matches(cssText, "url\\((?!['\"]?(?:data|http):)['\"]?([^'\"\\)]*)['\"]?\\)", RegexOptions.Multiline | RegexOptions.CultureInvariant);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var cssDirectory = Path.GetDirectoryName(cssFile);
                        var relativeToSourcePath = Path.GetRelativePath(rootDirectory, cssDirectory);
                        var fullPathToCssUrlFile = Path.GetFullPath(Path.Combine(cssDirectory, match.Groups[1].Value));
                        var urlResourceEscapedFilename = EscapeFile(Path.GetRelativePath(rootDirectory, fullPathToCssUrlFile));
                        cssText = cssText.Replace(match.Groups[1].Value, urlResourceEscapedFilename);
                    }

                    File.WriteAllText(targetCssFilePath, cssText);
                }
            }
        }

        private void ProcessJsMainModuleFile(XmlNode relationshipsNode, Guid productID, XmlDocument clientSideAssetsXml, int count, string rootDirectory, string file, in SharePointWebPart sharePointWebPart)
        {
            var fileContent = File.ReadAllText(file);
            var sanitizedName = TextManipulation.ToPascalCase(sharePointWebPart.Title);
            // TODO: use JSON schema to genrate the classes
            fileContent = fileContent
                .Replace("{{__GUID_ID__}}", sharePointWebPart.GuidId)
                .Replace("{{__VERSION__}}", sharePointWebPart.Version)
                .Replace("{{__NAME__}}", sanitizedName);

            var newFileName = $"{sanitizedName}_{applicationLoadContext.UniqueBuildString}.js";
            var newFilePath = CopyFileAndAddToXmlRel(relationshipsNode, productID, clientSideAssetsXml, count, rootDirectory, file, newFileName);
            File.WriteAllText(newFilePath, fileContent);
        }

        private string EscapeFile(string file)
        {
            var extension = Path.GetExtension(file);
            if (extension == ".dll")
            {
                /*
                 * Blazor in SPFx fix:
                 * Dll names must be the same as they loaded from URI because mono is loading them and we cannot rename them before it does
                 */
                return Path.GetFileName(file);
            }

            var directory = Path.GetDirectoryName(file);
            if (!string.IsNullOrEmpty(directory))
            {
                directory += "_";
            }
            var fileWithoutExtension = $"{directory}{Path.GetFileNameWithoutExtension(file)}";
            return fileWithoutExtension.Replace(Path.DirectorySeparatorChar, '_')
                + extension;
        }
    }
}
