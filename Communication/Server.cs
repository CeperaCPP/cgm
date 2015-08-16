using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public class Server
    {
        private string _server = null;
        private string _port = null;
        private string _user = null;
        private string _password = null;

        public string Srv
        {
            get { return this._server; }
            set { this._server = value; }
        }
        public string Port
        {
            get { return this._port; }
            set { this._port = value; }
        }
        public string User
        {
            get { return this._user; }
            set { this._user = value; }
        }
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

    }
}
