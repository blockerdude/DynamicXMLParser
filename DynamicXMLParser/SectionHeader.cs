
namespace DynamicXMLParser
{
    class SectionHeader
    {
        public string headerName { get; set; }
        public string original { get; set; }
        public int headerIndex { get; set; }
        public bool hasNameSpace { get; set; }

        public SectionHeader(string name)
        {
            headerName = name;
            original = name;
            headerIndex = 0;
            hasNameSpace = false;
        }
    }
}
