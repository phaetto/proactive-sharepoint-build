﻿namespace ProActive.SharePoint.Build.Services
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Linq;
    using System.Text.RegularExpressions;
    using ProActive.SharePoint.Build.Services.Strings;
    using ProActive.SharePoint.Build.Services.Contracts;
    using ProActive.SharePoint.Build.Services.Contexts;

    public sealed class CopyFilesToSpfxFolderService
    {
        private readonly string sourceFolder;
        private readonly string spfxOutputFolder;
        private readonly string contentTemplateFilesFolder;
        private readonly ApplicationLoadContext applicationLoadContext;

        public CopyFilesToSpfxFolderService(
            string sourceFolder,
            string spfxOutputFolder,
            string contentTemplateFilesFolder,
            ApplicationLoadContext applicationLoadContext
        )
        {
            this.sourceFolder = sourceFolder;
            this.spfxOutputFolder = spfxOutputFolder;
            this.contentTemplateFilesFolder = contentTemplateFilesFolder;
            this.applicationLoadContext = applicationLoadContext;
        }

        public void Process()
        {
            // Get all source files
            var sourceFiles = Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Where(x => Path.HasExtension(x))
                .Where(x => !x.EndsWith(Files.WebPartProduct, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => applicationLoadContext.SharePointWebParts.All(y => !Path.GetFileName(x).Equals(y.EntryPointFileName, StringComparison.InvariantCultureIgnoreCase)))
                .Where(x => applicationLoadContext.SharePointApplicationCustomizers.All(y => !Path.GetFileName(x).Equals(y.EntryPointFileName, StringComparison.InvariantCultureIgnoreCase)))
                .Where(x => applicationLoadContext.SharePointLibraries.All(y => !Path.GetFileName(x).Equals(y.EntryPointFileName, StringComparison.InvariantCultureIgnoreCase)));

            // Open the client-side-assets xml file
            var clientSideAssetsXml = new XmlDocument();
            var clientSideAssetsFileFullPath = $"{spfxOutputFolder}{Path.DirectorySeparatorChar}{Paths.ClientSideAssetsFile}";
            clientSideAssetsXml.Load(clientSideAssetsFileFullPath);
            var xmlnsManager = new XmlNamespaceManager(clientSideAssetsXml.NameTable);
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
                CopyFileAndAddToXmlRel(relationshipsNode, clientSideAssetsXml, count, file);

                ++count;
            }

            ProcessCssFiles(sourceFolder, sourceFilesArray);

            // TODO: Can't all entrypoints be like index.js? Do we have to provide the name?
            foreach (var webPart in applicationLoadContext.SharePointWebParts)
            {
                ProcessJsMainModuleFile(
                    relationshipsNode,
                    clientSideAssetsXml,
                    count,
                    contentTemplateFilesFolder,
                    Path.Combine(sourceFolder, webPart.EntryPointFileName),
                    webPart,
                    ClientSideType.WebPart);

                ++count;
            }

            foreach (var applicationCustomizer in applicationLoadContext.SharePointApplicationCustomizers)
            {
                ProcessJsMainModuleFile(
                    relationshipsNode,
                    clientSideAssetsXml,
                    count,
                    contentTemplateFilesFolder,
                    Path.Combine(sourceFolder, applicationCustomizer.EntryPointFileName),
                    applicationCustomizer,
                    ClientSideType.ApplicationCustomizer);

                ++count;
            }

            foreach (var library in applicationLoadContext.SharePointLibraries)
            {
                ProcessJsMainModuleFile(
                    relationshipsNode,
                    clientSideAssetsXml,
                    count,
                    contentTemplateFilesFolder,
                    Path.Combine(sourceFolder, library.EntryPointFileName),
                    library,
                    ClientSideType.Library);

                ++count;
            }

            clientSideAssetsXml.PreserveWhitespace = true;
            clientSideAssetsXml.Save(clientSideAssetsFileFullPath);
        }

        private string CopyFileAndAddToXmlRel(XmlNode relationshipsNode, XmlDocument clientSideAssetsXml, int idCount, string file, string targetFilename = null)
        {
            if (string.IsNullOrWhiteSpace(targetFilename))
            {
                targetFilename = EscapeFile(MakeRelativePath(sourceFolder, file));
            }
            var newFilePath = Path.Combine(spfxOutputFolder, "ClientSideAssets", targetFilename);

            File.Copy(file, newFilePath, overwrite: true);

            // Add file to XML doc
            var newNode = clientSideAssetsXml.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
            newNode.SetAttribute("Type", "http://schemas.microsoft.com/sharepoint/2012/app/relationships/clientsideasset");
            newNode.SetAttribute("Target", $"/ClientSideAssets/{targetFilename}");
            newNode.SetAttribute("Id", $"rel{idCount}");
            relationshipsNode.AppendChild(newNode);
            return newFilePath;
        }

        private void ProcessCssFiles(string rootDirectory, string[] sourceFiles)
        {
            foreach (var cssFile in sourceFiles.Where(x => Path.GetExtension(x) == ".css"))
            {
                var targetCssFilename = EscapeFile(MakeRelativePath(rootDirectory, cssFile));
                var targetCssFilePath = Path.Combine(spfxOutputFolder, "ClientSideAssets", targetCssFilename);
                var cssText = File.ReadAllText(targetCssFilePath);
                var matches = Regex.Matches(cssText, "url\\((?!['\"]?(?:data|http):)['\"]?([^'\"\\)]*)['\"]?\\)", RegexOptions.Multiline | RegexOptions.CultureInvariant);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var cssDirectory = Path.GetDirectoryName(cssFile);
                        var relativeToSourcePath = MakeRelativePath(rootDirectory, cssDirectory);
                        var fullPathToCssUrlFile = Path.GetFullPath(Path.Combine(cssDirectory, match.Groups[1].Value));
                        var urlResourceEscapedFilename = EscapeFile(MakeRelativePath(rootDirectory, fullPathToCssUrlFile));
                        cssText = cssText.Replace(match.Groups[1].Value, urlResourceEscapedFilename);
                    }

                    File.WriteAllText(targetCssFilePath, cssText);
                }
            }
        }

        private void ProcessJsMainModuleFile(XmlNode relationshipsNode, XmlDocument clientSideAssetsXml, int count, string contentTemplateFilesFolder, string entrypointFileName, in ISharePointEntryData sharePointWebPart, ClientSideType clientSideType)
        {
            string moduleFile;
            switch (clientSideType)
            {
                case ClientSideType.WebPart:
                    moduleFile = Files.WebPartMainModuleFileName;
                    break;
                case ClientSideType.ApplicationCustomizer:
                    moduleFile = Files.ApplicationCustomizerMainModuleFileName;
                    break;
                case ClientSideType.Library:
                    moduleFile = Files.LibraryMainModuleFileName;
                    break;
                default:
                    throw new NotImplementedException();
            }

            var mainModuleFileName = Path.Combine(contentTemplateFilesFolder, moduleFile);
            var entryPointfileContent = File.ReadAllText(entrypointFileName);
            var mainModuleFileContent = File.ReadAllText(mainModuleFileName);
            var sanitizedName = TextManipulation.ToPascalCase(sharePointWebPart.Title);
            // TODO: use JSON schema to genrate the classes
            mainModuleFileContent = mainModuleFileContent
                .Replace("{{__GUID_ID__}}", sharePointWebPart.GuidId.ToString())
                .Replace("{{__VERSION__}}", sharePointWebPart.Version)
                .Replace("{{__NAME__}}", sanitizedName)
                .Replace("{{__CODE__}}", entryPointfileContent)
                .Replace("{{__LIBRARIES__}}", sharePointWebPart.Dependencies != null
                    ? string.Join(", ", sharePointWebPart.Dependencies.Select(x => $"\"{x.Name}\""))
                    : string.Empty
                );

            var newFileName = $"{Path.GetFileNameWithoutExtension(sharePointWebPart.EntryPointFileName)}_{applicationLoadContext.UniqueBuildString}.js";
            var newFilePath = CopyFileAndAddToXmlRel(relationshipsNode, clientSideAssetsXml, count, entrypointFileName, newFileName);
            File.WriteAllText(newFilePath, mainModuleFileContent);
        }

        private string EscapeFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == ".dll")
            {
                /*
                 * Blazor in SPFx fix:
                 * Dll names must be the same as they loaded from URI because mono is loading them and we cannot rename them before it does
                 */
                return Path.GetFileName(fileName);
            }

            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory))
            {
                directory += "_";
            }
            var fileWithoutExtension = $"{directory}{Path.GetFileNameWithoutExtension(fileName)}";
            return $"{fileWithoutExtension.Replace(Path.DirectorySeparatorChar, '_')}_{applicationLoadContext.UniqueBuildString}{extension}";
        }

        private string MakeRelativePath(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return toPath;
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
