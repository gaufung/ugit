using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tindo.UgitCore
{
    public interface IProxyCommand
    {
        (int, string, string) Execute(string name, string argument);
    }
}
