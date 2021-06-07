using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Config
{
    public class AppSettings
    {
        public string RedisMaster { get; set; }
        public string RedisSlave { get; set; }
        public string Port { get; set; }
        public string PortSlave { get; set; }
        public string expiryTimeForCache { get; set; }
    }
}
