using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ugit
{
    internal class DefaultRemote : IRemote
    {
        private readonly IDataProvider dataProvider;

        public DefaultRemote(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        public void Fetch()
        {
            string prefix = Path.Join("refs", "heads");
            foreach (var (refName, _) in this.dataProvider.GetAllRefs(prefix))
            {
                Console.WriteLine(refName);
            }
        }
    }
}
