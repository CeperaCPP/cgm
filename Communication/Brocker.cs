using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterSystems.Data.CacheClient;
using InterSystems.Data.CacheTypes;
using SysGM;

namespace Communication
{
    public class Brocker
    {
        private CacheConnection _connection = null;
        private API _cacheapi = null;


        ///====================================================================
        /// Конструктор по-умолчанию
        ///====================================================================
        public Brocker()
        {
            CacheConnection _connection = new CacheConnection();
            _connection.ConnectionString = CacheConnection.ConnectDlg();
            _connection.Open();
            _cacheapi = new API(_connection);
        }
        ///====================================================================
        /// конструктор для готового подключения
        ///====================================================================
        public Brocker(CacheConnection conn)
        {
            _connection = conn;
            _cacheapi = new API(_connection);
        }
        ///====================================================================
        /// СОздание подключения по строке подключения
        ///====================================================================
        public Brocker(string connString)
        {
            CacheConnection _connection = new CacheConnection(connString);
            _cacheapi = new API(_connection);
        }
        ///====================================================================
        /// Диструктор
        ///====================================================================
        ~Brocker()
        {
            //_cacheapi.Dispose();
            //_connection.CloseAllObjects();
            //_connection.CloseAllUnreachableObjects();
            //if (_connection.State.ToString() == "Open")
            //{
                //_cacheapi.Connection.Close();
                //_connection.Close();
            //}
            _cacheapi.Dispose();
            //_connection.Dispose();
        }
        //public string 
        ///====================================================================
    }
}
