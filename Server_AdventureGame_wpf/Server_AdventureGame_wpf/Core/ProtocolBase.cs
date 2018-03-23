using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Core
{
    public abstract class ProtocolBase
    {
        public abstract string Expression { get; set; }
        public abstract string Name { get; set; }

        public abstract ProtocolBase Decode(byte[] bufferRead, int start, int length);
        public abstract byte[] Encode();

    }
}
