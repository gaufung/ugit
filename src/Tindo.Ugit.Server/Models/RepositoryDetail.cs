using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace Tindo.Ugit.Server.Models
{
    public class RepositoryDetail : Repository
    {
        public IDirectoryContents DirectoryContent { get; set; }

        public string Path { get; set; }

        public List<PathDetail> PathDetails
        {
            get
            {
                var segments = Path.Split(new char[] { '\\', '/' });
                List<PathDetail> details = new List<PathDetail>();
                for(int i = 0; i < segments.Length; i++)
                {
                    if (i == 0)
                    {
                        details.Add(new PathDetail()
                        {
                            RootToPath = segments[i],
                            Path = segments[i]
                        });
                    }
                    else
                    {
                        details.Add(new PathDetail()
                        {
                            RootToPath = details[i - 1].RootToPath + "/" + segments[i],
                            Path = segments[i]
                        });
                    }
                }

                return details;
            }
            
        }
    }
}
