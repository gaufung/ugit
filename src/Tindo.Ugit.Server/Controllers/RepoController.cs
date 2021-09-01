using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;

namespace Tindo.Ugit.Server.Controllers
{
    [Route("repo")]
    public class RepoController : Controller
    {
        private readonly ILogger _logger;

        private UgitServerOptions _serverOption;

        private readonly IFileOperator _fileOperator;

        private readonly UgitDatabaseContext _ugitDatabaseContext;

        public RepoController(
            IOptions<UgitServerOptions> serverOption,
            IFileSystem fileSystem, 
            UgitDatabaseContext databaseContext, 
            ILogger<RepoController> logger)
        {
            _serverOption = serverOption.Value;
            _fileOperator = new PhysicalFileOperator(fileSystem);
            _ugitDatabaseContext = databaseContext;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult Index(int id)
        {
            return View();
        }

        [HttpGet("{id}/tree/{directory?}")]
        public IActionResult Get(int id, string directory)
        {
            return View("Index");
        }
    }
}
