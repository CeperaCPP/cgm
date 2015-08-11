using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using InterSystems.Data.CacheClient;
//using InterSystems.Data.CacheTypes;
//using SysGM;
using VISMLib;

namespace Communication
{
    public class Brocker
    {
        private VisM _vism = null;
        private bool _connstatus = false;

        ///====================================================================
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        ///====================================================================
        public Brocker()
        {
            _vism = new VisM();
        }
        ///====================================================================
        /// <summary>
        /// Деструктор
        /// </summary>
        ///====================================================================
        ~Brocker()
        {

        }
        ///====================================================================
        /// <summary>
        /// Подключиться к базе
        /// </summary>
        ///====================================================================
        public bool Connect(string Server, string Port, string User = "", string Password = "")
        {
            int res;
            res = _vism.SetServer("CN_IPTCP:" + Server + "[" + Port + "]:"+User+":"+Password);
            if (res == 0)
            {
                _connstatus = true;
                _vism.ErrorTrap = true;
                _vism.OnError += _vism_OnError;
            }
            return _connstatus;

        }
        ///====================================================================
        /// <summary>
        /// Обработчик ошибок выполнения кода через vism
        /// </summary>
        ///====================================================================        
        void _vism_OnError()
        {
            _vism.Execute("d BACK^%ETN");
        }
        
        ///====================================================================
        /// <summary>
        /// Отключиться от базы
        /// </summary>
        ///====================================================================
        public void Disconnect()
        {
            _vism.SetServer("");
            _vism.DeleteConnection();
            _connstatus = false;

        }
        ///====================================================================
        /// <summary>
        /// Инициализация списка областей БД во временный глобал
        /// </summary>
        ///====================================================================
        public void InitNSP()
        {
            string cmd;
            ClearBUF();
            cmd = "set rs=##class(%ResultSet).%New()";
            _vism.Execute(cmd);
            cmd = "set rs.ClassName=\"%SYS.Namespace\"";
            _vism.Execute(cmd);
            cmd = "set rs.QueryName=\"List\"";
            _vism.Execute(cmd);
            cmd = "set sc=rs.Execute()";
            _vism.Execute(cmd);
            cmd = "while rs.%Next() { set ^CacheTempCGM($J,rs.Data(\"Nsp\"))=\"\" }";
            _vism.Execute(cmd);
            cmd = "k rs,sc";
            _vism.Execute(cmd);

        }
        ///====================================================================
        /// <summary>
        /// Очистка буфера
        /// </summary>
        ///====================================================================
        public void ClearBUF()
        {
            string cmd;
            cmd = "kill ^CacheTempCGM($J)";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// <summary>
        /// Следующая область
        /// </summary>
        ///====================================================================
        public string GetNextNSP(string NSP = "")
        {// переписать на использование Next
            _vism.P1 = dataBuf();
            _vism.P2 = NSP;
            string cmd = "S VALUE=$O(@P1@(P2))";
            _vism.Execute(cmd);
            return _vism.VALUE;
        }
        ///====================================================================
        /// <summary>
        /// Предыдущая область
        /// </summary>
        ///====================================================================
        public string GetPreviousNSP(string NSP = "")
        {// переписать на использование Next
            _vism.P1 = dataBuf();
            _vism.P2 = NSP;
            string cmd = "S VALUE=$O(@P1@(P2),-1)";
            _vism.Execute(cmd);
            return _vism.VALUE;
        }
        ///====================================================================
        /// <summary>
        /// Инициализация временного буфера глобалов для заданной области
        /// </summary>
        ///====================================================================
        public void InitGlb(string NSP = "", string SysGlb = "0")
        {
            string cmd;           
            ClearBUF();
            _vism.P1 = NSP;                 //NameSpace 
            _vism.P2 = "*";                 //Mask 
            _vism.P3 = SysGlb;              //SystemGlobals
            //_vism.P4 = "";                  //UnavailableDatabases
            //_vism.P5 = "";                  //Index           
            cmd = "set rs=##class(%ResultSet).%New()";
            _vism.Execute(cmd);
            cmd = "set rs.ClassName=\"%SYS.GlobalQuery\"";
            _vism.Execute(cmd);
            cmd = "set rs.QueryName=\"NameSpaceListChui\"";
            _vism.Execute(cmd);
            cmd = "set sc=rs.Execute(P1,P2,P3)";
            _vism.Execute(cmd);
            cmd = "while rs.%Next() { set nam=$P(rs.Data(\"Name\"),\"(\",1),^CacheTempCGM($J,nam)=\"\" }";
            _vism.Execute(cmd);
            cmd = "k rs,sc";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// <summary>
        /// Следующий глобал из буфера
        /// </summary>
        ///====================================================================
        public string NextGlobal(string Glb = "", int Type = 1)
        {
            string res;
            string buf;
            buf = dataBuf();
            res = Next(buf, Glb, "1", Type);
            return res;
        }
        ///====================================================================
        /// <summary>
        /// Предыдущий глобал из буфера
        /// </summary>
        ///====================================================================
        public string PreviousGlobal(string Glb = "", int Type = 1)
        {
            string res;
            string buf;
            buf = dataBuf();
            res = Next(buf, Glb, "-1", Type);
            return res;
        }
        ///====================================================================
        /// <summary>
        /// Имя буфера
        /// </summary>
        ///====================================================================
        private string dataBuf()
        {
            return "^CacheTempCGM($J)";
        }
        ///====================================================================
        /// <summary>
        /// Следующий произвольный глобал
        /// Order - порядок следования "1" -прямой, "-1" -обратный
        /// Type - тип представления (1 - послойный, иначе - в видет таблицы)
        /// </summary>
        ///====================================================================
        public string Next(string Glb = "",string Nod="",string Order = "1",int Type = 1)
        {
            string cmd;
            string node;
            _vism.P0 = Glb;
            _vism.P1 = Nod;
            if (Type == 1)
            {
                cmd = "S VALUE=$O(@P0@(P1),"+Order+")";
            }
            else
            {
                cmd = "S VALUE=$Q(@P0@(P1)," + Order + ")";                
            }
            _vism.Execute(cmd);
            node = _vism.VALUE;
            return node;
        }
        ///====================================================================
        /// <summary>
        /// Статус соединения
        /// </summary>
        ///====================================================================
        public bool getConnectionStatus()
        {
            return _connstatus;
        }
        ///====================================================================
        /// <summary>
        /// Вернуть строку для таблицы
        /// </summary>
        ///====================================================================
        public string getRow(string glb = "")
        {
            return "";
        }
        ///====================================================================
        /// <summary>
        /// Поиск (пока идея)
        /// </summary>
        ///====================================================================
        public string Find()
        {
            string cmd;
            // P0 - глобал, P1 - узел, P2 - строка для поиска, P3 - параметры поиска (1 в узлах, 2 в значениях, 3 и там и там)
            cmd = "s nod=P1 f  s nod=$O(@P0@(nod),1) Q:nod=\"\"  I (P3=1!P3=3),$F(nod,P2) S VALUE=nod Q  I (P3=2!P3=3),$F($G(@P0@(nod)),P2) S VALUE=nod Q";
            cmd="s nod=$NA(@P0@(P1)) f  s nod=$Q(@nod,1) Q:nod=\"\"  I (P3=1!P3=3),$F(nod,P2) S VALUE=nod Q  I (P3=2!P3=3),$F($G(@nod),P2) S VALUE=nod Q";

            return "";
        }
        ///====================================================================
    }
}
