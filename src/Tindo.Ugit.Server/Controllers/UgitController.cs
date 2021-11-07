using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tindo.Ugit.Server.Controllers
{
    [AllowAnonymous]
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
        public IActionResult GetRefs(string repo, [FromQuery] string prefix = "", [FromQuery] bool deref = true)
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            var refs = dataProvider.GetAllRefs(prefix, deref).ToDictionary(kv => kv.Item1, kv => kv.Item2);
            var content = JsonSerializer.Serialize(refs);
            var data = JsonSerializer.SerializeToUtf8Bytes(refs);
            return new FileContentResult(data, "application/octet-stream");
        }

        [HttpPost("{repo}/ref/{refName}")]
        public async Task<IActionResult> UpdateRef(string repo, string refName, [FromQuery]bool deref=true)
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            using var ms = new MemoryStream();
            await this.Request.Body.CopyToAsync(ms);
            byte[] body = ms.ToArray();
            RefValue refValue = JsonSerializer.Deserialize<RefValue>(body);
            dataProvider.UpdateRef(refName, refValue, deref);
            return Ok();
        }

        [HttpGet("{repo}/objects/{oid}")]
        public IActionResult GetObject(string repo, string oid, [FromQuery] string expected = "")
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            var data = string.IsNullOrWhiteSpace(expected) ? dataProvider.ReadObject(oid) : dataProvider.GetObject(oid, expected);
            return new FileContentResult(data, "application/octet-stream");
        }

        [HttpPost("{repo}/objects/{oid}")]
        public async Task<IActionResult>  WriteObject(string repo, string oid)
        {
            string repoPath = Path.Join(_serverOption.RepositoryDirectory, repo);
            IDataProvider dataProvider = new LocalDataProvider(this._fileOperator, repoPath);
            using var ms = new MemoryStream();
            await this.Request.Body.CopyToAsync(ms);
            byte[] data = ms.ToArray();
            dataProvider.WriteObject(oid, data);
            return Ok();
        }
    }
}
