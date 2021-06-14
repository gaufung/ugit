using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tindo.UgitCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tindo.UgitServer.Controllers
{
    [ApiController]
    public class GitController : ControllerBase
    {
        private readonly UgitServer ugitServer;

        private readonly IFileSystem fileSystem;

        private readonly ILoggerFactory loggerFactory;

        public GitController(IOptions<UgitServer> ugitServerOption, IFileSystem fileSystem,
            ILoggerFactory loggerFactory)
        {
            this.ugitServer = ugitServerOption.Value;
            this.fileSystem = fileSystem;
            this.loggerFactory = loggerFactory;
        }

        [HttpGet("{repo}/objects/{objectId}")]
        public ActionResult GetObject(string repo, string objectId)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            var filepath = Path.Join(repoPath,Constants.GitDir, Constants.Objects, objectId);
            var bytes = fileOperator.Read(filepath);
            return File(bytes, "application/octet-stream");
        }

        [HttpPost("{repo}/objects/{objectId}")]
        public ActionResult WriteObject(string repo, string objectId)
        {
            var filePath = Path.Join(this.ugitServer.RootPath, repo, Constants.GitDir, Constants.Objects, objectId);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream(2048))
            {
                Request.Body.CopyToAsync(ms).Wait();
                bytes = ms.ToArray();
            }
            fileOperator.Write(filePath, bytes);
            return Ok();
        }

        [HttpGet("{repo}/refs/{prefix?}")]
        public ActionResult<Dictionary<string, RefValue>> GetAllRefs(string repo, string prefix,
            [FromQuery] bool deref = true)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            IDataProvider dataProvider = new LocalDataProvider(fileOperator, repoPath, loggerFactory);
            prefix = Path.Join("refs", prefix);
            return dataProvider.GetAllRefs(prefix, deref)
                .ToDictionary(kv => kv.Item1, kv => kv.Item2);
        }

        [HttpGet("{repo}")]
        public ActionResult<Dictionary<string, RefValue>> GetAllRefs(string repo, [FromQuery] bool deref = true)
        {
            return GetAllRefs(repo, "", deref);
        }

        [HttpPost("{repo}/refs/{*refname}")]
        public ActionResult UpdateRef(string repo, string refname, [FromQuery]bool deref=true)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            IDataProvider dataProvider = new LocalDataProvider(fileOperator, repoPath, loggerFactory);
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream(2048))
            {
                Request.Body.CopyToAsync(ms).Wait();
                bytes = ms.ToArray();
            }
            var refValue = JsonSerializer.Deserialize<RefValue>(bytes);
            dataProvider.UpdateRef(Path.Join("refs", refname), refValue, deref);
            return Ok();
        }
        
    }
}