using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloWarsTools
{
    class HWModel
    {
        public string Name;
        public HWUgxResource MeshResource;

        public HWModel(string name, HWUgxResource meshResource) {
            Name = name;
            MeshResource = meshResource;
        }
    }
}
