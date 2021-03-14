using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebZipIt.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IWebHostEnvironment hostEnvironment;

        public IndexModel(IWebHostEnvironment hostEnvironment)
        {
            this.hostEnvironment = hostEnvironment;
        }

        public void OnGet()
        {
        }

        public FileStreamResult OnGetDownloadBots()
        {
            var botsFolderPath = Path.Combine(hostEnvironment.ContentRootPath, "bots");
            var botFilePaths = Directory.GetFiles(botsFolderPath);
            var zipFileMemoryStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Update, leaveOpen: true))
            {
                foreach (var botFilePath in botFilePaths)
                {
                    var botFileName = Path.GetFileName(botFilePath);
                    var entry = archive.CreateEntry(botFileName);
                    using (var entryStream = entry.Open())
                    using (var fileStream = System.IO.File.OpenRead(botFilePath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
            return File(zipFileMemoryStream, "binary/octet-stream", "Bots.zip");
        }
    }
}
