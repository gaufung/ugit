using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using System.IO.Abstractions;
using Tindo.Ugit.Server.Models;
using System.IO;

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
            IFileProvider fileProvider = new PhysicalFileProvider(_serverOption.RepositoryDirectory, Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
            var  repo = _ugitDatabaseContext.Repositories.FirstOrDefault(r => r.Id == id);
            IDirectoryContents directoryContent = fileProvider.GetDirectoryContents(repo.Name);
            return View(new RepositoryDetail() { DirectoryContent = directoryContent, Path = repo.Name, Id = repo.Id, Name=repo.Name });
        }

        [HttpGet("{id}/tree/{**directory}")]
        public IActionResult Get(int id, string directory)
        {
            IFileProvider fileProvider = new PhysicalFileProvider(Path.Join(_serverOption.RepositoryDirectory), Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
            var repo = _ugitDatabaseContext.Repositories.FirstOrDefault(r => r.Id == id);
            IDirectoryContents directoryContent = fileProvider.GetDirectoryContents(directory);
            return View("Index", new RepositoryDetail() { DirectoryContent = directoryContent, Path = directory, Id = id, Name = repo.Name});
        }


        [HttpGet("{id}/delete")]
        [HttpPost("{id}/delete")]
        public IActionResult Delete(int id)
        {
            var repo = _ugitDatabaseContext.Repositories.FirstOrDefault(r => r.Id == id);
            string repoName = repo.Name;
            string repoPhysicalFolder = Path.Join(_serverOption.RepositoryDirectory, repoName);
            this._fileOperator.Delete(repoName, false);
            _ugitDatabaseContext.Repositories.Remove(repo);
            _ugitDatabaseContext.SaveChanges();
            return Redirect("/");
        }
    }
}
