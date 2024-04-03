using pwither.formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testApp
{
    [BitSerializable]
    internal class Test
    {
        public int TestInt {  get; set; }
        public string TestStr { get; set; }
        public DateTime DateTime { get; set; }
        public InnerTest InnerTest;
        public int I;
    }
}
