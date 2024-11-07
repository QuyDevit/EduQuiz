using EduQuiz.Areas.Admin.Models;
using elFinder.NetCore;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Route("el-finder-file-system")]
    public class FileSystemController : Controller
    {
        private IWebHostEnvironment _environment;
        private FileSystemConfig _fileSystemConfig;
        public FileSystemController(IWebHostEnvironment environment,IOptions<FileSystemConfig> fileSystemConfig) {
            _environment = environment;
            _fileSystemConfig = fileSystemConfig.Value;
        }
        [Route("connector")]
        public async Task< IActionResult> Connector()
        {
            var connector = GetConnector();
            return await connector.ProcessAsync(Request);
        }
        [Route("thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }
        private Connector GetConnector()
        {
            string pathRoot = _fileSystemConfig.RootFolder;
            var driver = new FileSystemDriver();
            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            string[] subfolders = { "img", "music", "templates" };

            foreach (var folder in subfolders)
            {
                string folderPath = Path.Combine(_environment.WebRootPath, pathRoot, folder);
                string folderUrl = $"{uri.Scheme}://{uri.Authority}/{pathRoot}/{folder}/";
                string urlThumb = $"{uri.Scheme}://{uri.Authority}/el-finder-file-system/thumb/";

                // Add a root volume for each folder
                if (Directory.Exists(folderPath))
                {
                    var root = new RootVolume(folderPath, folderUrl, urlThumb)
                    {
                        IsReadOnly = false,
                        IsLocked = false,
                        Alias = folder,
                        ThumbnailSize = 100
                    };
                    driver.AddRoot(root);
                }
            }
            return new Connector(driver)
            {
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}
