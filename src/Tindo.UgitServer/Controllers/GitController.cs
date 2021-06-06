using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Tindo.UgitCore;
using Microsoft.Extensions.Logging;

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
        public ActionResult<byte[]> GetObject(string repo, string objectId)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            IDataProvider dataProvider = new LocalDataProvider(fileOperator, repoPath, loggerFactory);
            return dataProvider.GetObject(objectId);
        }

        [HttpPost("{repo}/objects/{objectId}")]
        public ActionResult WriteObject(string repo, string objectId, [FromBody] byte[] bytes)
        {
            var filePath = Path.Join(this.ugitServer.RootPath, repo, objectId);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            fileOperator.Write(filePath, bytes);
            return Ok();
        }

        [HttpGet("{repo}/refs/{prefix}")]
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

        [HttpPost("{repo}/refs/{refname}")]
        public ActionResult UpdateRef(string repo, string @ref, [FromBody] RefValue refValue, [FromQuery]bool deref=true)
        {
            string repoPath = Path.Join(this.ugitServer.RootPath, repo);
            IFileOperator fileOperator = new PhysicalFileOperator(this.fileSystem);
            IDataProvider dataProvider = new LocalDataProvider(fileOperator, repoPath, loggerFactory);
            dataProvider.UpdateRef(@ref, refValue, deref);
            return Ok();
        }
        
    }
}