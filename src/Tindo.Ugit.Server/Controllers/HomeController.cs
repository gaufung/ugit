using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tindo.Ugit.Server.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System.IO.Abstractions;
using System.IO;
using Tindo.Ugit;

namespace Tindo.Ugit.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string RepositoryDirectory;

        private readonly IFileSystem _fileSystem;

        public HomeController(IOptions<UgitServerOptions> serverOption, IFileSystem fileSystem, ILogger<HomeController> logger)
        {
            RepositoryDirectory = serverOption.Value.RepositoryDirectory;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public IActionResult Index()
        {
            IFileProvider dataProvider = new PhysicalFileProvider(RepositoryDirectory);

            var repos = dataProvider.GetDirectoryContents("")
                        .Where(f => f.IsDirectory)
                        .OrderByDescending(f => f.LastModified)
                        .Select(f => new RepositoryModel { Name = f.Name })
                        .ToArray();

            return View(repos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RepositoryModel repo)
        {
            if (!ModelState.IsValid)
            {
                return View(repo);
            }

            string folderPath = Path.Join(RepositoryDirectory, repo.Name);
            if (_fileSystem.Directory.Exists(folderPath))
            {
                _fileSystem.Directory.Delete(folderPath, true);
            }

            IDataProvider dataProvider = new LocalDataProvider(new PhysicalFileOperator(_fileSystem), folderPath);
            dataProvider.Init();

            return Redirect("/home");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
