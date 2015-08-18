using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VISMLib;
using Localization;
namespace Communication
{
    public class Broker
    {
        ///====================================================================
        /// Описание полей класса
        ///====================================================================
        #region Поля
        private VisM _vism = null;
        private bool _connstatus = false;
        private string _nsp = null;
        private string _global = null;
        //========================================
        // заменить на использование объета Server
        private string _server = null;
        private string _port = null;
        private string _user = null;
        private string _pass = null;
        //========================================
        private Stack<string> _subscripts;
        private Stack<string> _levels;
        private string _startSS = null;
        private string _endSS = null;
        private int _sizeSS = 0;
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
        ///====================================================================
        public string Global
        {
            get { return _global; }
            set { this._global = value; }
        }
        ///====================================================================
        public string Server
        {
            get { return _server; }
            set { this._server = value; }
        }
        ///====================================================================
        public string Port
        {
            get { return _port; }
            set { this._port = value; }
        }
        ///====================================================================
        public string User
        {
            get { return _user; }
            set { this._user = value; }
        }
        ///====================================================================
        public string Password
        {
            get { return _pass; }
            set { this._pass = value; }
        }
        ///====================================================================
        /// <summary>
        /// Максимальный размер буфера. Фактически кол-во элементов
        /// </summary>
        ///====================================================================
        public int Size
        {
            get { return _sizeSS; }
            set { this._sizeSS = value; }
        }
        ///====================================================================
        /// <summary>
        /// Начальный элемент
        /// </summary>
        ///====================================================================
        public string Start
        {
            get { return _startSS; }
            set { this._startSS = value; }
        }
        ///====================================================================
        /// <summary>
        /// Конечный элемент
        /// </summary>
        ///====================================================================
        public string End
        {
            get { return _endSS; }
            set { this._endSS = value; }
        }
        ///====================================================================
        public bool ConnectionStatus
        {
            get { return _connstatus; }
        }
        ///====================================================================
        public string GlobalPath
        {
            get { return ""; }
        }
        #endregion
        ///====================================================================
        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        ///====================================================================
        public Broker()
        {
            _vism = new VisM();
            _subscripts = new Stack<string>();
            _levels = new Stack<string>();
            _server = "";
            _port = "";
            _user = "";
            _pass = "";

            _startSS = "";
            _endSS = "";

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
        public bool Connect()
        {
            int res;
            if (Server=="") throw new Exception(Translate.Brocker_Connect_Empty_Server);
            if (Port == "") throw new Exception(Translate.Brocker_Connect_Empty_Port);          
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
            this.Server = "";
            this.Port = "";
            this.User = "";
            this.Password = "";
            _vism.SetServer(this.Server);
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
        public void InitNSP(string Start ="", int Size = 0, int Direction = 1)
        {
            string cmd;
            ClearBUF();
            _nsp = null;
            _global = null;
            _vism.P0 = "%SYS.Namespace";
            _vism.P1 = "List";
            _vism.P2 = "Nsp";
            _vism.P3 = "";
            _subscripts.Clear();
            cmd = "set rs=##class(%ResultSet).%New()";
            _vism.Execute(cmd);
            cmd = "set rs.ClassName=P0";
            _vism.Execute(cmd);
            cmd = "set rs.QueryName=P1";
            _vism.Execute(cmd);
            cmd = "set sc=rs.Execute()";
            _vism.Execute(cmd);
            cmd = "while rs.%Next() { set ^CacheTempCGMBuf($J,rs.Data(P2))=P3 }";
            _vism.Execute(cmd);
            cmd = "k rs,sc";
            _vism.Execute(cmd);

            cmd = "S P0=$NA(^CacheTempCGMBuf($J))";
            _vism.Execute(cmd);
            _vism.P1 = "";
            _vism.P2=Start;
            _vism.P3=Direction;
            _vism.P4=Size;
            cmd = "S revers=$S(P3=1:-1,1:1)";
            _vism.Execute(cmd);
            cmd = "S nod=$S(P2'=P1:$O(@P0@(P2),revers),1:P1)";
            _vism.Execute(cmd);
            cmd = "F  {S nod=$O(@P0@(nod),P3) Q:nod=P1  S ^CacheTempCGM($J,nod)=P1 Q:'$I(P4,-1)  }";
            _vism.Execute(cmd);
            cmd = "k nod,revers";
            _vism.Execute(cmd);

        }
        ///====================================================================
        /// <summary>
        /// Инициализация временного буфера со списком глобалов для заданной 
        /// области
        /// </summary>
        ///====================================================================
        public void InitGlb(string NSP = "", string SysGlb = "0", string Start = "", int Size = 0, int Direction = 1)
        {
            string cmd;
            ClearBUF();
            _global = null;
            _subscripts.Clear();
            this.NameSpace = NSP;
            _vism.P0 = "%SYS.GlobalQuery";
            _vism.P1 = "NameSpaceListChui";
            _vism.P2 = NSP;                 //NameSpace 
            _vism.P3 = "*";                 //Mask 
            _vism.P4 = SysGlb;              //SystemGlobals
            _vism.P5 = "Name";
            _vism.P6 = "(";
            _vism.P7 = " ";
            _vism.P8 = "";
            
            cmd = "set rs=##class(%ResultSet).%New()";
            _vism.Execute(cmd);
            cmd = "set rs.ClassName=P0";
            _vism.Execute(cmd);
            cmd = "set rs.QueryName=P1";
            _vism.Execute(cmd);
            cmd = "set sc=rs.Execute(P2,P3,P4)";
            _vism.Execute(cmd);
            cmd = "while rs.%Next() { set nam=$p($p(rs.Data(P5),P6,1),P7,1) set ^CacheTempCGMBuf($J,nam)=P8 }";
            _vism.Execute(cmd);
            cmd = "k rs,sc";
            _vism.Execute(cmd);

            cmd = "S P0=$NA(^CacheTempCGMBuf($J))";
            _vism.Execute(cmd);
            _vism.P1 = "";
	        _vism.P2=Start;
	        _vism.P3=Direction;
            _vism.P4 = Size;

            cmd = "S revers=$S(P3=1:-1,1:1)";
            _vism.Execute(cmd);
            cmd = "S nod=$S(P2'=P1:$O(@P0@(P2),revers),1:P1)";
            _vism.Execute(cmd);
            cmd = "F  { S nod=$O(@P0@(nod),P3) Q:nod=P1  S ^CacheTempCGM($J,nod)=P1 Q:'$I(P4,-1)  }";
            _vism.Execute(cmd);
            cmd = "k nod,revers";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера узла дерева
        /// </summary>
        ///====================================================================
        private void InitSub(string Glb = "",int ViewType=1, string Start = "", int Size = 0, int Direction = 1)
        {
            string cmd;
            ClearBUF();
            _global = Glb;
            _vism.P0 = getGlbName(_nsp,_global);
            _vism.P1 = "";
            _vism.P2 = Start;
            _vism.P3 = Direction;
            _vism.P4 = Size;

            cmd = "S revers=$S(P3=1:-1,1:1)";
            _vism.Execute(cmd);
            if (ViewType == 1)
            {
                cmd = "S nod=$S(P2'=P1:$O(@P0@(P2),revers),1:P1) ";
                _vism.Execute(cmd);
                cmd = "F  { S nod=$O(@P0@(nod),P3) Q:nod=P1  S ^CacheTempCGM($J,nod)=P1 Q:'$I(P4,-1) }";
                _vism.Execute(cmd);
            }
            else
            {
                cmd = "S nod=$S(P2'=P1:$Q(@P0@(P2),revers),1:P0)";
                _vism.Execute(cmd);
                cmd = "if nod=P1 s nod=P0";
                _vism.Execute(cmd);
                cmd = "F  { S nod=$Q(@nod,P3) Q:nod=P1  S ^CacheTempCGM($J,nod)=P1 Q:'$I(P4,-1)  }";
                _vism.Execute(cmd);
            }         
            cmd = "k nod";
            _vism.Execute(cmd);
        }
        ///====================================================================
        /// <summary>
        /// Вернуть имя глобала с подузлами в виде строки.
        /// Заполняет данные в переменную P1
        /// </summary>
        ///====================================================================
        private string getGlbName(string NSP = "",string Glb = "")
        {
            string cmd;
            string item = null;
            _vism.P0 = NSP;
            cmd = "S P1=$NA(^|P0|"+Glb+")";
            _vism.Execute(cmd);
            Stack<string> local = null;
            if (_subscripts.Count > 0)
            {
                local = new Stack<string>(_subscripts);
                while (local.Count > 0)
                {
                    item = local.Pop();
                    _vism.P2 = item;
                    cmd = "S P1=$NA(@P1@(P2))";
                    _vism.Execute(cmd);
                }
            }
            return _vism.P1;
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
            cmd = "kill ^CacheTempCGMBuf($J)";
            _vism.Execute(cmd);
        }
        ///====================================================================
        #endregion
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
        #region Навигация
        ///====================================================================
        /// <summary>
        /// Следующий глобал из буфера
        /// </summary>
        ///====================================================================
        public string NextGlobal(string Glb = "", bool Type = true, bool Revers = false)
        {
            string res;
            string buf;
            string order;
            order = "1";
            if (Revers) order = "-1";
            buf = dataBuf();
            res = Next(buf, Glb, order, Type);
            return res;
        }
        ///====================================================================
        /// <summary>
        /// Предыдущий глобал из буфера
        /// </summary>
        ///====================================================================
        public string PreviousGlobal(string Glb = "", bool Type = true, bool Revers = false)
        {
            string res;
            string buf;
            string order;
            order = "-1";
            if (Revers) order = "1";
            buf = dataBuf();
            res = Next(buf, Glb, order, Type);
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
        /// Type - тип представления (true - послойный, false - в видет таблицы)
        /// </summary>
        ///====================================================================
        public string Next(string Glb = "", string Nod = "", string Order = "1", bool Type = true)
        {
            string cmd;
            string node;
            _vism.P0 = Glb;
            _vism.P1 = Nod;
            if (Type)
            {
                cmd = "S VALUE=$O(@P0@(P1)," + Order + ")";
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
        /// Движение вверх (к корню) глобала
        /// </summary>
        ///====================================================================
        public string Up(string ShowSys = "1")
        {
            if ((null == this.NameSpace) ||
                (null == this.Global))
            {
                InitNSP();
            }
            else
            {
                if (_subscripts.Count == 0)
                {
                    InitGlb(_nsp, ShowSys);
                }
                else
                {
                    _subscripts.Pop();
                    InitSub(_global);
                }
            }
            return _levels.Pop();
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
                    InitSub(item);
                }
                else
                {
                    _subscripts.Push(item);
                    InitSub(_global);
                }
            }
            _levels.Push(item);
        }
        ///====================================================================
        #endregion
        ///====================================================================
    }
}
