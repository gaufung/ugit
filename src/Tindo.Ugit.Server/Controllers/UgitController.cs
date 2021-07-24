using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tindo.Ugit.Server.Controllers
{
    [Route("api")]
    public class UgitController : Controller
    {
        private readonly ILogger<UgitController> _logger;

        private readonly UgitServerOptions _serverOption;

        public UgitController(IOptions<UgitServerOptions> serverOption, ILogger<UgitController> logger)
        {
            _serverOption = serverOption.Value;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return Ok("ugit api" + _serverOption.RepositoryDirectory);
        }
    }
}
