using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Tindo.Ugit.Server.Models;

namespace Tindo.Ugit.Server.Controllers
{
    [Route("repo")]
    public class RepoController : Controller
    {
        private readonly ILoggerFactory _loggerFactory;

        private UgitServerOptions _serverOption;

        private readonly IFileOperator _fileOperator;

        private readonly UgitDatabaseContext _ugitDatabaseContext;

        public RepoController(
            IOptions<UgitServerOptions> serverOption,
            IFileSystem fileSystem,
            UgitDatabaseContext databaseContext,
            ILoggerFactory loggerFactory)
        {
            _serverOption = serverOption.Value;
            _fileOperator = new PhysicalFileOperator(fileSystem);
            _ugitDatabaseContext = databaseContext;
            _loggerFactory = loggerFactory;
        }

        [HttpGet("{id}")]
        public IActionResult Index(int id)
        {
            IFileProvider fileProvider = new PhysicalFileProvider(_serverOption.RepositoryDirectory, Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
            var repo = _ugitDatabaseContext.Repositories.FirstOrDefault(r => r.Id == id);
            if (repo == null)
            {
                return NotFound($"Repository with Id {id} couldn't been found");
            }

            CheckoutMasterBranch(repo.Name);
            IDirectoryContents directoryContent = fileProvider.GetDirectoryContents(repo.Name);
            return View(new RepositoryDetail() { DirectoryContent = directoryContent, Path = repo.Name, Id = repo.Id, Name = repo.Name });
        }

        private void CheckoutMasterBranch(string repoName)
        {
            var dataProvider = new LocalDataProvider(_fileOperator, Path.Join(_serverOption.RepositoryDirectory, repoName));
            var treeOperation = new TreeOperation(dataProvider, _loggerFactory.CreateLogger<TreeOperation>());
            var commitOperation = new CommitOperation(dataProvider, treeOperation, _loggerFactory.CreateLogger<CommitOperation>());
            var branchOperation = new BranchOperation(dataProvider, _loggerFactory.CreateLogger<BranchOperation>());
            var checkoutOperation = new CheckoutOperation(dataProvider, treeOperation, commitOperation, branchOperation, _loggerFactory.CreateLogger<CheckoutOperation>());
            try
            {
                checkoutOperation.Checkout("master");
            }
            catch(UgitException)
            {

            }
        }

        [HttpGet("{id}/tree/{**directory}")]
        public IActionResult Get(int id, string directory)
        {
            IFileProvider fileProvider = new PhysicalFileProvider(Path.Join(_serverOption.RepositoryDirectory), Microsoft.Extensions.FileProviders.Physical.ExclusionFilters.None);
            var repo = _ugitDatabaseContext.Repositories.FirstOrDefault(r => r.Id == id);
            IDirectoryContents directoryContent = fileProvider.GetDirectoryContents(directory);
            return View("Index", new RepositoryDetail() { DirectoryContent = directoryContent, Path = directory, Id = id, Name = repo.Name });
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
