using System.Collections.Generic;
using SharedArea.Middles;

namespace SharedArea.Commands
{
    public class Request
    {
        public Dictionary<string, string> Headers { get; set; }
        public Packet Packet { get; set; }
    }
}