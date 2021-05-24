using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Tindo.UgitServer.Controllers
{
    [ApiController]
    public class GitController : ControllerBase
    {
        private readonly UgitServer ugitServer;

        public GitController(IOptions<UgitServer> ugitServerOption)
        {
            this.ugitServer = ugitServerOption.Value;
        }
        
        [HttpGet("{repo}")]
        public ActionResult<string> GetRepo(string repo)
        {
            return Path.Join(ugitServer.RootPath, repo);
        }
    }
}