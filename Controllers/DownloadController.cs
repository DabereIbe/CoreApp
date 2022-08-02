using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApp.Controllers
{
    public class DownloadController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DownloadController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string filename)
        {
            var download = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "Inquiries" + Path.DirectorySeparatorChar.ToString();

            var fullpath = Path.Combine(download, filename);

            MemoryStream memory = new MemoryStream();

            using (FileStream fileStream = new FileStream(fullpath, FileMode.Open))
            {
                fileStream.CopyTo(memory);
            }

            memory.Position = 0;
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileNameWithoutExtension(filename);
            return File(memory, contentType, fileName + ".pdf");
        }
    }
}
