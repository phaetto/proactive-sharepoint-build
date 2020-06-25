namespace ProActive.SharePoint.Build.Services
{
    using ProActive.SharePoint.Build.Services.Contexts;
    using ProActive.SharePoint.Build.Services.Contracts;
    using System;
    using System.IO;
    using System.Xml;

    public sealed class CreateSpfxFolderStructureService
    {
        private readonly string spfxOutputFolder;
        private readonly string creationTemplateFolder;
        private readonly ApplicationLoadContext applicationLoadContext;

        public CreateSpfxFolderStructureService(
            string spfxOutputFolder,
            string creationTemplateFolder,
            ApplicationLoadContext applicationLoadContext
        )
        {
            this.spfxOutputFolder = spfxOutputFolder;
            this.creationTemplateFolder = creationTemplateFolder;
            this.applicationLoadContext = applicationLoadContext;
        }

        public void Process()
        {
            // Create the basic folder structure
            var nonRenamedtemplateFiles = new string[]
            {
                "[Content_Types].xml",
                "AppManifest.xml",
                "ClientSideAssets.xml",
                "ClientSideAssets.xml.config.xml",
            };
            foreach (var templateFile in nonRenamedtemplateFiles)
            {
                File.Copy(Path.Combine(creationTemplateFolder, templateFile), Path.Combine(spfxOutputFolder, templateFile));
            }

            // Setup app manifest
            SetupAppManifest(applicationLoadContext.SharePointProduct, Path.Combine(spfxOutputFolder, "AppManifest.xml"));

            // Setup feature and webpart folders
            foreach (var webPart in applicationLoadContext.SharePointWebParts)
            {
                var jsonManifest = GenerateManifestJson(webPart, Path.Combine(creationTemplateFolder, "manifest.json"));

                // Setup features
                var featureXmlFilePath = Path.Combine(spfxOutputFolder, $"feature_{webPart.GuidId}.xml");
                File.Copy(Path.Combine(creationTemplateFolder, "feature_ID.xml"), featureXmlFilePath);
                SetupFeatureXmlFile(applicationLoadContext.SharePointProduct, webPart, featureXmlFilePath);

                var featureXmlConfigXmlFilePath = Path.Combine(spfxOutputFolder, $"feature_{webPart.GuidId}.xml.config.xml");
                File.Copy(Path.Combine(creationTemplateFolder, "feature_ID.xml.config.xml"), featureXmlConfigXmlFilePath);
                SetupFeatureXmlConfigFile(featureXmlConfigXmlFilePath);

                // Setup WebPart folder
                var webPartOutputDirectory = Path.Combine(spfxOutputFolder, webPart.GuidId);
                Directory.CreateDirectory(webPartOutputDirectory);
                var webPartFilePath = Path.Combine(webPartOutputDirectory, $"WebPart_{webPart.GuidId}.xml");
                File.Copy(Path.Combine(creationTemplateFolder, "WebPart", "WebPart_ID.xml"), webPartFilePath);
                SetupWebpartFile(
                    webPart,
                    webPartFilePath,
                    jsonManifest);
            }

            // Setup .rels folder
            var relsOutputDirectory = Path.Combine(spfxOutputFolder, "_rels");
            Directory.CreateDirectory(relsOutputDirectory);
            var nonRenamedRelsTemplateFiles = new string[]
            {
                ".rels",
                "ClientSideAssets.xml.rels",
                "AppManifest.xml.rels",
            };
            foreach (var templateFile in nonRenamedRelsTemplateFiles)
            {
                File.Copy(Path.Combine(creationTemplateFolder, "_rels", templateFile), Path.Combine(relsOutputDirectory, templateFile));
            }

            var idCount = 1;
            var appManifestXmlFilePath = Path.Combine(relsOutputDirectory, "AppManifest.xml.rels");
            foreach (var webPartAttribute in applicationLoadContext.SharePointWebParts)
            {
                var featureRelsXmlFilePath = Path.Combine(relsOutputDirectory, $"feature_{webPartAttribute.GuidId}.xml.rels");
                File.Copy(Path.Combine(creationTemplateFolder, "_rels", "feature_ID.xml.rels"), featureRelsXmlFilePath);
                SetupRelsFeatureXmlFile(webPartAttribute, featureRelsXmlFilePath, idCount);

                AddEntryToAppManifestXmlFile(webPartAttribute, appManifestXmlFilePath, $"rel{idCount}");
                ++idCount;
            }

            Directory.CreateDirectory(Path.Combine(spfxOutputFolder, "ClientSideAssets"));
        }

        private void SetupAppManifest(SharePointProduct product, string filePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/2012/app/manifest");
            var node = (XmlElement)xmlDocument.SelectSingleNode("/ns:App", xmlnsManager);
            node.SetAttribute("ProductID", product.GuidId);
            node.SetAttribute("Version", product.Version);
            node.SetAttribute("Name", product.Name);
            var titleNode = (XmlElement)xmlDocument.SelectSingleNode("/ns:App/ns:Properties/ns:Title", xmlnsManager);
            titleNode.InnerText = product.Name;
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }

        private string GenerateManifestJson(SharePointWebPart webPart, string templateFilePath)
        {
            var manifestFileContents = File.ReadAllText(templateFilePath);
            var sanitizedName = TextManipulation.ToPascalCase(webPart.Title);
            return manifestFileContents // TODO: do it properly with a JSON serializer
                .Replace("{{__GuidId__}}", webPart.GuidId)
                .Replace("{{__Title__}}", webPart.Title)
                .Replace("{{__Description__}}", webPart.Description)
                .Replace("{{__FILENAME__}}", $"{sanitizedName}_{applicationLoadContext.UniqueBuildString}.js")
                .Replace("{{__NAME__}}", sanitizedName)
                .Replace("\n", "")
                .Replace("\r", "")
                .Trim();
            // TODO: change the rest of the file (ids, etc)
        }

        private void SetupWebpartFile(
            SharePointWebPart webPart,
            string filePath,
            string jsonComponentManifest)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/");
            var node = (XmlElement)xmlDocument.SelectSingleNode("/ns:Elements/ns:ClientSideComponent", xmlnsManager);
            node.SetAttribute("Id", webPart.GuidId);
            node.SetAttribute("Name", webPart.Title);
            node.SetAttribute("ComponentManifest", jsonComponentManifest);
            var moduleNode = (XmlElement)xmlDocument.SelectSingleNode("/ns:Elements/ns:Module", xmlnsManager);
            moduleNode.SetAttribute("Name", webPart.Title);
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }

        private void SetupFeatureXmlFile(
            SharePointProduct product,
            SharePointWebPart webPart,
            string filePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/");
            var node = (XmlElement)xmlDocument.SelectSingleNode("/ns:Feature", xmlnsManager);
            node.SetAttribute("Title", $"{webPart.Title} Feature");
            node.SetAttribute("Description", $"A feature which activates the Client-Side WebPart named {webPart.Title}");
            node.SetAttribute("Id", webPart.GuidId);
            node.SetAttribute("Version", product.Version); // Not sure why this is not the web part version, but that's how it's generated
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }

        private void SetupFeatureXmlConfigFile(string filePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.microsoft.com/sharepoint/2012/app/partconfiguration");
            var node = (XmlElement)xmlDocument.SelectSingleNode("/ns:AppPartConfig/ns:Id", xmlnsManager);
            node.InnerText = Guid.NewGuid().ToString();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }

        private void SetupRelsFeatureXmlFile(
            SharePointWebPart webPart,
            string filePath,
            int idCount)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/package/2006/relationships");
            var partConfigurationNode = (XmlElement)xmlDocument.SelectSingleNode("/ns:Relationships/ns:Relationship[@Type='http://schemas.microsoft.com/sharepoint/2012/app/relationships/partconfiguration']", xmlnsManager);
            partConfigurationNode.SetAttribute("Target", $"/feature_{webPart.GuidId}.xml.config.xml");
            partConfigurationNode.SetAttribute("Id", $"rf{2 * idCount}");
            var elementManifestNode = (XmlElement)xmlDocument.SelectSingleNode("/ns:Relationships/ns:Relationship[@Type='http://schemas.microsoft.com/sharepoint/2012/app/relationships/feature-elementmanifest']", xmlnsManager);
            elementManifestNode.SetAttribute("Target", $"/{webPart.GuidId}/WebPart_{webPart.GuidId}.xml");
            elementManifestNode.SetAttribute("Id", $"rf{2 * idCount + 1}");
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }

        private void AddEntryToAppManifestXmlFile(SharePointWebPart webPart, string filePath, string id)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlnsManager.AddNamespace("ns", "http://schemas.openxmlformats.org/package/2006/relationships");

            var relationshipsNode = (XmlElement)xmlDocument.SelectSingleNode("/ns:Relationships", xmlnsManager);

            var newEntryNode = xmlDocument.CreateElement("Relationship", "http://schemas.openxmlformats.org/package/2006/relationships");
            newEntryNode.SetAttribute("Type", "http://schemas.microsoft.com/sharepoint/2012/app/relationships/manifest-feature");
            newEntryNode.SetAttribute("Target", $"/feature_{webPart.GuidId}.xml");
            newEntryNode.SetAttribute("Id", id);

            relationshipsNode.AppendChild(newEntryNode);

            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Save(filePath);
        }
    }
}
