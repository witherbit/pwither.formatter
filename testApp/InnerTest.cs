using pwither.formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testApp
{
    [BitSerializable]
    internal class InnerTest
    {
        public byte[] Data { get; set; }
    }
}
