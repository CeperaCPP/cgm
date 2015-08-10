using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterSystems.Data.CacheClient;
using InterSystems.Data.CacheTypes;
using SysGM;
using VISMLib;

namespace Communication
{
    public class Brocker
    {
        private VisM _vism = null;
        private bool _connstatus = false;

        ///====================================================================
        /// Конструктор по-умолчанию
        ///====================================================================
        public Brocker()
        {
            _vism = new VisM();
        }
        ///====================================================================
        /// Диструктор
        ///====================================================================
        ~Brocker()
        {

        }
        ///====================================================================
        /// Подключиться к базе
        ///====================================================================
        public bool Connect(string Server, string Port, string User = "", string Password = "")
        {
            int res;
            res = _vism.SetServer("CN_IPTCP:" + Server + "[" + Port + "]:"+User+":"+Password);
            if (res == 0)
            {
                _connstatus = true;
                _vism.Execute("s $ZT=\"BACK^%ETN\"");
            }
            return _connstatus;
            //_vism.Connect("CN_IPTCP:127.0.0.1[1972]");

        }
        ///====================================================================
        /// Отключиться от базы
        ///====================================================================
        public void Disconnect()
        {
            _vism.SetServer("");
            _vism.DeleteConnection();
            _connstatus = false;

        }
        ///====================================================================
        /// Инициализация списка областей БД во временный глобал
        ///====================================================================
        public void InitNSP()
        {
            string cmd;
            //if (_vism.ConnectionState == 0) return;
            ClearNSP();
            cmd = "f i=1:1:$ZU(90,0) s:$ZU(90,2,0,i)'=\"\" ^CacheTempNSP($J,$ZU(90,2,0,i))=\"\"";
            _vism.Execute(cmd);

        }
        ///====================================================================
        /// Очистка списка областей
        ///====================================================================
        public void ClearNSP()
        {
            string cmd;            
            cmd = "kill ^CacheTempNSP($J)";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// Следующая область
        ///====================================================================
        public string GetNextNSP(string NSP = "")
        {
            _vism.P1 = "^CacheTempNSP($J)";
            _vism.P2 = NSP;
            string cmd = "S VALUE=$O(@P1@(P2))";
            _vism.Execute(cmd);
            return _vism.VALUE;
        }
        ///====================================================================
        /// Предыдущая область
        ///====================================================================
        public string GetPreviousNSP(string NSP = "")
        {
            _vism.P1 = "^CacheTempNSP($J)";
            _vism.P2 = NSP;
            string cmd = "S VALUE=$O(@P1@(P2),-1)";
            _vism.Execute(cmd);
            return _vism.VALUE;
        }
        ///====================================================================
        /// Инициализация списка глобалов для заданной области
        ///====================================================================
        public void InitGlbList(string NSP = "")
        {
            string cmd;
            cmd = "k ^CacheTempJ($J)";
            _vism.Execute(cmd);
            _vism.P1 = NSP;
            cmd = "S:$G(P1)=\"\" P1=$ZU(5)";
            _vism.Execute(cmd);
            cmd = "D INT^%GD(P1)";
            _vism.Execute(cmd);
            cmd = "S rc=$$Fetch^%GD(\"*\",1,0)";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// Статус соединения
        ///====================================================================
        public bool getConnectionStatus()
        {
            return _connstatus;
        }
        ///====================================================================
    }
}
