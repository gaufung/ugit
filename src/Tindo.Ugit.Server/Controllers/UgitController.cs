using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;

namespace Tindo.Ugit.Server.Controllers
{
    public class UgitController : Controller
    {
        private readonly ILogger<UgitController> _logger;

        private readonly UgitServerOptions _serverOption;

        private readonly IFileOperator _fileOperator;

        public UgitController(IOptions<UgitServerOptions> serverOption, IFileSystem fileSystem, ILogger<UgitController> logger)
        {
            _serverOption = serverOption.Value;
            _fileOperator = new PhysicalFileOperator(fileSystem);
            _logger = logger;
        }

        public IActionResult Index()
        {
            return Ok("ugit api" + _serverOption.RepositoryDirectory);
        }

        [HttpGet("{repo}/refs")]
        public IActionResult GetRefs(string repo, [FromQuery]string prefix = "", [FromQuery]bool deref=true)
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            var refs = dataProvider.GetAllRefs(prefix, deref).ToDictionary(kv=>kv.Item1, kv=>kv.Item2);
            var content = JsonSerializer.Serialize(refs);
            var data = JsonSerializer.SerializeToUtf8Bytes(refs) ;
            return new FileContentResult(data, "application/octet-stream");
        }

        [HttpGet("{repo}/objects/{oid}")]
        public IActionResult GetObject(string repo, string oid, [FromQuery]string expected="")
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            var data = string.IsNullOrWhiteSpace(expected) ? dataProvider.ReadObject(oid) : dataProvider.GetObject(oid, expected);
            return new FileContentResult(data, "application/octet-stream");
        }
    }
}
