using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss_password
{
    class ServerInfo
    {
        public string server;
        public string server_port;
        public string method;
        public string password;
        public string remarks = "";
        public bool auth;

        public ServerInfo()
        {
            this.remarks = "";
            this.auth = false;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to ServerInfo return false.
            ServerInfo si = obj as ServerInfo;
            if ((System.Object)si == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (server == si.server) && (server_port == si.server_port) && (method == si.method) && (password == si.password);
        }

        public bool RequiredFieldsAllSet()
        {
            return server != null && server_port != null && method != null && password != null;
        }
    }
}
