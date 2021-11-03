using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloWarsTools
{
    public class HWModel
    {
        public string Name;
        public HWUgxResource Resource;

        public HWModel(string name, HWUgxResource resource) {
            Name = name;
            Resource = resource;
        }
    }
}
