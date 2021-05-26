using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tindo.UgitCore;

namespace Tindo.UgitServer.Controllers
{
    [ApiController]
    public class GitController : ControllerBase
    {
        private readonly UgitServer ugitServer;

        private readonly IFileSystem fileSystem;

        public GitController(IOptions<UgitServer> ugitServerOption, IFileSystem fileSystem)
        {
            this.ugitServer = ugitServerOption.Value;
            this.fileSystem = fileSystem;
        }

        [HttpGet("{repo}/objects/{objectId}")]
        public ActionResult<byte[]> GetObject(string repo, string objectId)
        {
            var filePath = Path.Join(this.ugitServer.RootPath, repo, Constants.Objects, objectId);
            return this.fileSystem.File.ReadAllBytes(filePath);
        }

        [HttpGet("{repo}/objects/{objectId}/expect/{expected}")]
        public ActionResult<byte[]> GetObject(string repo, string objectId, string expected)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IDataProvider dataProvider = new LocalDataProvider(this.fileSystem, repoPath);
            return dataProvider.GetObject(objectId, expected);
        }

        [HttpPost("{repo}/objects/{objectId}")]
        public ActionResult WriteObject(string repo, string objectId, [FromBody] byte[] bytes)
        {
            var filePath = Path.Join(this.ugitServer.RootPath, repo, objectId);
            this.fileSystem.File.WriteAllBytes(filePath, bytes);
            return Ok();
        }

        [HttpGet("{repo}/refs/{prefix}")]
        public ActionResult<Dictionary<string, RefValue>> GetAllRefs(string repo, string prefix,
            [FromQuery] bool deref = true)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IDataProvider dataProvider = new LocalDataProvider(this.fileSystem, repoPath);
            prefix = Path.Join("refs", prefix);
            return dataProvider.GetAllRefs(prefix, deref)
                .ToDictionary(kv => kv.Item1, kv => kv.Item2);
        }

        [HttpPost("{repo}/refs/{refname}")]
        public ActionResult UpdateRef(string repo, string @ref, [FromBody] RefValue refValue, [FromQuery]bool deref=true)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IDataProvider dataProvider = new LocalDataProvider(this.fileSystem, repoPath);
            dataProvider.UpdateRef(@ref, refValue, deref);
            return Ok();
        }
        
    }
}