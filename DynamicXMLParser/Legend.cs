using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXMLParser
{
    class Legend
    {
        public List<string> columnHeaders { get; set; }
        public List<List<SectionHeader>> sectionHeaders { get; set; }

        public Legend()
        {

        }
    }
}
