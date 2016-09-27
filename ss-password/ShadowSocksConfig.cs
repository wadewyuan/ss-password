using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss_password
{
    class ShadowSocksConfig
    {
        public List<ServerInfo> configs;
        public string strategy;
        public int index = 0;
        public bool global;
        public bool enabled;
        public bool shareOverLan;
        public bool isDefault;
        public int localPort;
        public string pacUrl;
        public bool useOnlinePac;
        public bool availabilityStatistics;
        public bool autoCheckUpdate;
        public string logViewer;
    }
}
