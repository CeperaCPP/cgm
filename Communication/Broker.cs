using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VISMLib;

namespace Communication
{
    public class Broker
    {
        private VisM _vism = null;
        private bool _connstatus = false;
        private string _nsp = null;
        private string _global = null;
        private string _subscripts = null;
        private Stack<string> _stack;
        ///====================================================================
        /// <summary>
        /// Имя активной области
        /// </summary>
        ///====================================================================
        public string NameSpace
        {
            get { return _nsp; }
            set { this._nsp = value; }
        }
        public string Global
        {
            get { return _global; }
            set { this._global = value; }
        }
        public string SubScripts
        {
            get { return _subscripts; }
            set { this._subscripts = value; }
        }
        ///====================================================================
        /// <summary>
        /// Текущий путь (глобальная ссылка)
        /// </summary>
        ///====================================================================
        public string GlobalPath
        {
            get {
                if (null == _global) return _nsp;
                else if (null == _subscripts) return "^|\"" + _nsp + "\"|" + _global;
                else return "^|\"" + _nsp + "\"|" + _global + "(" + _subscripts + ")";
            }            
        }
        ///====================================================================
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        ///====================================================================
        public Broker()
        {
            _vism = new VisM();
            _stack = new Stack<string>();
        }
        ///====================================================================
        /// <summary>
        /// Деструктор
        /// </summary>
        ///====================================================================
        ~Broker()
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
        /// 
        ///====================================================================
        #region Инициализация буфера данными
        ///====================================================================
        /// <summary>
        /// Инициализация списка областей БД во временный глобал
        /// </summary>
        ///====================================================================
        public void InitNSP()
        {
            string cmd;
            ClearBUF();
            _nsp = null;
            _global = null;
            _subscripts = null;
            _stack.Clear();
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
        /// Инициализация временного буфера со списком глобалов для заданной 
        /// области
        /// </summary>
        ///====================================================================
        public void InitGlb(string NSP = "", string SysGlb = "0")
        {
            string cmd;
            ClearBUF();
            _global = null;
            _subscripts = null;
            _stack.Clear();
            this.NameSpace = NSP;
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
            cmd = "while rs.%Next() { set nam=$p($p(rs.Data(\"Name\"),\"(\",1),\" \",1),^CacheTempCGM($J,nam)=\"\" }";
            _vism.Execute(cmd);
            cmd = "k rs,sc";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера узла дерева
        /// </summary>
        ///====================================================================
        private void InitSub()
        {
            throw new NotImplementedException();
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
        #endregion
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
        /// <summary>
        /// Движение вверх (к корню) глобала
        /// </summary>
        ///====================================================================
        public void Up(string ShowSys = "1")
        {
            if (null == this.NameSpace)
            {
                InitNSP();
            }
            else
            {
                if (null == this.Global)
                {
                    InitNSP();

                }
                else
                {
                    _subscripts = getSubScript();
                    if (null == _subscripts)
                    {
                        InitGlb(_nsp, ShowSys);
                    }
                    else
                    {
                        _subscripts = getSubScript(); //this.SubScripts + "," + item; // переработать
                    }
                }
            }
        }
        ///====================================================================
        /// <summary>
        /// Движение в глубь (к листьям) глобала
        /// </summary>
        ///====================================================================
        public void Down(string item="", string ShowSys = "1")
        {
            if (""==item) return;
            if (null == this.NameSpace)
            {
                InitGlb(item, ShowSys);
            }
            else
            {
                if (null == this.Global)
                {
                    this.Global = item;
                }
                else
                {
                    this.SubScripts = getSubScript(item); //this.SubScripts + "," + item; 
                    InitSub();
                }
            }
        }
        ///====================================================================
        /// <summary>
        /// вернуть строку индексов
        /// </summary>
        ///====================================================================
        private string getSubScript(string item="")
        {
            string result= null;
            if (_stack.Count > 0)
            {
                if ("" == item)
                {
                    _stack.Pop();
                }
                else
                {
                    _stack.Push(item);
                }
                if (_stack.Count > 0)
                {
                    result = String.Join("\",\"", _stack.ToArray());
                }
            }
            return result;
        }
        ///====================================================================
    }
}
