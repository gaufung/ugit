﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tindo.Ugit.Server.Models;

namespace Tindo.Ugit.Server.Controllers
{
    public class HomeController : Controller
    {
        private ILoggerFactory _loggerFactory;


        private readonly string RepositoryDirectory;

        private readonly IFileSystem _fileSystem;

        private readonly UgitDatabaseContext _dbContext;

        public HomeController(IOptions<UgitServerOptions> serverOption, IFileSystem fileSystem, 
            UgitDatabaseContext dbContext, ILoggerFactory loggerFactory)
        {
            RepositoryDirectory = serverOption.Value.RepositoryDirectory;
            _loggerFactory = loggerFactory;
            _fileSystem = fileSystem;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var repos = _dbContext.Repositories
                .OrderByDescending(f => f.LastModified)
                .ToArray();

            return View(repos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Repository repo)
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
            IInitOperation operation = new InitOperation(dataProvider, _loggerFactory.CreateLogger<InitOperation>());
            operation.Init();

            Repository repostiory = new Repository
            {
                Name = repo.Name,
                Description = repo.Description,
                LastModified = DateTime.UtcNow,
            };

            this._dbContext.Repositories.Add(repostiory);
            await this._dbContext.SaveChangesAsync();

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
