using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Tindo.Ugit.Server.Models
{
    public class RepositoryDetail : Repository
    {
        public IDirectoryContents DirectoryContent { get; set; }

        public string Path { get; set; }
    }
}
