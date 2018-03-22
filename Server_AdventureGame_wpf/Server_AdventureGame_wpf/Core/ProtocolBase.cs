using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_AdventureGame_wpf.Core
{
    public abstract class ProtocolBase
    {
        public string Expression { get; set; } = "Exception is not initialized.";
        public string Name { get; set; } = "NULLPROTOCOL";

        public abstract ProtocolBase Decode(byte[] bufferRead, int start, int length);
        public abstract byte[] Encode();

    }
}
