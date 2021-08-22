using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tindo.Ugit.Server.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;

namespace Tindo.Ugit.Server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly string RepositoryDirectory;

        public HomeController(IOptions<UgitServerOptions> serverOption, ILogger<HomeController> logger)
        {
            RepositoryDirectory = serverOption.Value.RepositoryDirectory;
            _logger = logger;
        }

        public IActionResult Index()
        {
            IFileProvider dataProvider = new PhysicalFileProvider(RepositoryDirectory);

            return View();
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
