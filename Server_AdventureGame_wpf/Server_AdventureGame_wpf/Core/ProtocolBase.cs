using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Core
{
    public abstract class ProtocolBase
    {
        public string Expression { get; set; } = "No Expression";
        public string Name { get; set; } = "Test";

        public abstract ProtocolBase Decode(byte[] bufferRead, int start, int length);
        public abstract byte[] Encode();

    }
}
