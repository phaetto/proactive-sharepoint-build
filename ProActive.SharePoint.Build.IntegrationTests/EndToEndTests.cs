namespace ProActive.SharePoint.Build.IntegrationTests
{
    using ProActive.SharePoint.Build.Services;
    using Shouldly;
    using System.IO;
    using Xunit;

    public class EndToEndTests
    {
        private readonly string SourceFolder = Path.GetFullPath($"Artifacts{Path.DirectorySeparatorChar}EndToEnd{Path.DirectorySeparatorChar}Source");
        private readonly string OutputFolder = Path.GetFullPath($"Artifacts{Path.DirectorySeparatorChar}EndToEnd{Path.DirectorySeparatorChar}Target");
        private readonly string TemplateCreationFolder = Path.GetFullPath($"Templates{Path.DirectorySeparatorChar}Creation");
        private readonly string BuildContentFolder = Path.GetFullPath($"Content");

        [Fact]
        public void Process_WhenProcessingJsonWebpartSpec_ThenItSuccessfullyFindsTheWebparts()
        {
            var initSpfxFolderStructureService = new InitSpfxFolderStructureService(SourceFolder, OutputFolder);

            var applicationLoadContext = initSpfxFolderStructureService.Process();

            applicationLoadContext.ShouldNotBeNull();
            applicationLoadContext.SharePointWebParts.Length.ShouldBe(2);
        }

        [Fact]
        public void Process_WhenRunningTheWholeProcess_ThenItDoesNotFail()
        {
            var initSpfxFolderStructureService = new InitSpfxFolderStructureService(SourceFolder, OutputFolder);
            var applicationLoadContext = initSpfxFolderStructureService.Process();
            var createSpfxFolderStructureService = new CreateSpfxFolderStructureService(OutputFolder, TemplateCreationFolder, applicationLoadContext);
            createSpfxFolderStructureService.Process();
            var copyFilesToSpfxFolderService = new CopyFilesToSpfxFolderService(SourceFolder, OutputFolder, BuildContentFolder, applicationLoadContext);
            copyFilesToSpfxFolderService.Process();
        }
    }
}
