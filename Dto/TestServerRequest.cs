using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IsMyArmaServerVulnerableApi.Dto
{
    public class TestServerRequest
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
    }
}
