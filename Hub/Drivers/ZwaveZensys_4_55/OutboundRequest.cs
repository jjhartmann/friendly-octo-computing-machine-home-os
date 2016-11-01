using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Drivers.ZwaveZensys
{
    public class OutboundRequest
    {
        public byte NodeId { get; private set; }
        public byte[] Data { get; private set; }
        public string Description { get; private set; }

        public OutboundRequest(byte nodeId, byte[] data, string desc)
        {
            this.NodeId = nodeId;
            this.Data = data;
            this.Description = desc;
        }
    }
}
