using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Communication;

namespace cpm
{
    public partial class mainForm : Form
    {
        private Broker _leftbrocker = null;
        private Broker _rightbrocker = null;

        //private string _serverL = "127.0.0.1" ; //"WIN10";
        private string _serverL;
        private string _portL;
        private string _userL;
        private string _passwordL;

        private string _serverR;
        private string _portR;
        private string _userR;
        private string _passwordR;

        private string _showsys;
        ///====================================================================
        /// <summary>
        /// Конструктор
        /// </summary>
        ///====================================================================
        public mainForm()
        {
            InitializeComponent();
            _leftbrocker = new Broker();
            _rightbrocker = new Broker();
            readConfig();
        }
        ///====================================================================
        /// <summary>
        /// Прочитать настройки
        /// </summary>
        ///====================================================================

        private void readConfig()
        {
            // левая панель
            _serverL = "win10";
            _portL = "1972";
            _userL = "_system";
            _passwordL = "SYS";
            // правая панель
            _serverR = "win10";
            _portR = "1972";
            _userR = "_system";
            _passwordR = "SYS";
            // Показать системные глобалы
            _showsys = "1";
        }

        ///====================================================================
        /// <summary>
        /// Изменение размера функциональных клавиш
        /// </summary>
        ///=====================================================================
        private void toolStripBottom_Resize(object sender, EventArgs e)
        {
            //MessageBox.Show("hello");            
        }
        ///====================================================================
        /// <summary>
        /// Обработчик загрузки формы
        /// </summary>
        ///====================================================================
        private void mainForm_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("hello");
            if (_leftbrocker.Connect(_serverL, _portL, _userL, _passwordL))
            {
                _leftbrocker.InitNSP();
                PanelInit(_leftbrocker, toolStripLeftNSP, listViewLeft);
            }
            if (_rightbrocker.Connect(_serverR, _portR, _userR, _passwordR))
            {
                _rightbrocker.InitNSP();
                PanelInit(_rightbrocker, toolStripRightNSP, listViewRight);
            }
        }
        ///====================================================================
        /// <summary>
        /// Закрытие формы
        /// </summary>
        ///====================================================================
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_leftbrocker.getConnectionStatus())
            {
                _leftbrocker.ClearBUF();
                _leftbrocker.Disconnect();
            }
            if (_rightbrocker.getConnectionStatus())
            {
                _rightbrocker.ClearBUF();
                _rightbrocker.Disconnect();
            }
        }
        ///====================================================================
        /// <summary>
        /// Обработчик события отрисовки контнейнера
        /// </summary>
        ///====================================================================
        private void splitContainer1_Paint(object sender, PaintEventArgs e)
        {            
            //PanelInit(_leftbrocker);
            //PanelInit(_rightbrocker);
            
        }
        ///====================================================================
        /// <summary>
        /// Инициализация панели
        /// </summary>
        ///====================================================================
        private void PanelInit(Broker brokerobj,ToolStripComboBox cbNSP, ListView lstview)
        {
            string nsp = brokerobj.GetNextNSP("");
            while (nsp != "")
            {                
                cbNSP.Items.Add(nsp);
                lstview.Items.Add(nsp);
                nsp = brokerobj.NextGlobal(nsp);
            }
        }
        ///====================================================================
        /// <summary>
        /// Клик по элементу в левой панели
        /// </summary>
        ///====================================================================
        private void listViewLeft_Click(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            initGlb(lv, _leftbrocker, _showsys);
        }
        ///====================================================================
        /// <summary>
        /// Клик по элементу в правой панели
        /// </summary>
        ///====================================================================
        private void listViewRight_Click(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            initGlb(lv, _rightbrocker, _showsys);
        }
        ///====================================================================
        /// <summary>
        /// Инициализация глобала
        /// </summary>
        ///====================================================================
        private void initGlb(ListView lv, Broker brokerobj, string ShowSys="1")
        {
            string val;
            if (lv.SelectedItems.Count == 0) return;
            val = lv.SelectedItems[0].Text;
            if (".." == val) {
                brokerobj.Up();
            }
            if (null==brokerobj.NameSpace)
            {
                brokerobj.InitGlb(val, ShowSys);
            }
            else
            {
                if (null==brokerobj.Global)
                {
                    brokerobj.Global = val;
                }
                else
                {
                    brokerobj.SubScripts = brokerobj.SubScripts + "," + val; // переработать
                }
            }

        }
        ///====================================================================
        /// Блок кода отвечающий за обработку нажатия клавиш
        ///====================================================================
        #region Обработчики нажатия клавиш для панелей
        ///====================================================================
        /// <summary>
        /// Нажатие клавиши на левой панели
        /// </summary>
        ///====================================================================
        private void listViewLeft_KeyDown(object sender, KeyEventArgs e)
        {
            object[] args = null;
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyDown");
            if (null == method) return;
            args = new object[] { listViewLeft, _leftbrocker, e };
            method.Invoke(this, args); 

        }
        ///====================================================================
        /// <summary>
        /// Отпустили клавишу на левой панели
        /// </summary>
        ///====================================================================
        private void listViewLeft_KeyUp(object sender, KeyEventArgs e)
        {
            object[] args = null;
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyUp");
            if (null == method) return;
            args = new object[] { listViewLeft, _leftbrocker, e };
            method.Invoke(this, args); 
        }
        ///====================================================================
        /// <summary>
        /// Нажатие клавиши на правой панели
        /// </summary>
        ///====================================================================
        private void listViewRight_KeyDown(object sender, KeyEventArgs e)
        {
            object[] args = null;
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyDown");
            if (null == method) return;
            args= new object[] {listViewRight, _rightbrocker ,e};
            method.Invoke(this, args);            
        }
        ///====================================================================
        /// <summary>
        /// Отпустили клавишу на правой панели
        /// </summary>
        ///====================================================================
        private void listViewRight_KeyUp(object sender, KeyEventArgs e)
        {
            object[] args = null;
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyUp");
            if (null == method) return;
            args = new object[] { listViewRight, _rightbrocker, e };
            method.Invoke(this, args);  
        }
        #endregion
        ///====================================================================
        /// 
        ///====================================================================
        #region Обработчики отдельно взятых клавиш
        ///====================================================================
        /// <summary>
        /// Обработка нажатия на клавишу Enter
        /// </summary>
        ///====================================================================
        public void ReturnKeyDown(ListView lv, Broker brocker, KeyEventArgs evnt)
        {
            //MessageBox.Show("Enter Down");
            initGlb(lv, brocker, _showsys);

        }
        ///====================================================================
        /// <summary>
        /// Обработка отпускания клавиши Enter
        /// </summary>
        ///====================================================================
        public void ReturnKeyUp(ListView lv, Broker broker, KeyEventArgs evnt)
        {
            ReLoad(lv, broker);
        }
        ///====================================================================
        #endregion
        ///====================================================================
        /// <summary>
        /// Движение в глубь (к листьям) глобала
        /// </summary>
        ///====================================================================
        public void ReLoad(ListView lv, Broker broker)
        {
            string glb;
            lv.Items.Clear();
            if (null != broker.NameSpace) lv.Items.Add("..");
            glb = broker.NextGlobal("");
            while (glb != "")
            {
                //cbNSP.Items.Add(glb);
                lv.Items.Add(glb);
                glb = broker.NextGlobal(glb);
            }
        }///====================================================================
        /// <summary>
        /// Движение в глубь (к листьям) глобала
        /// </summary>
        ///====================================================================
        public void Down(ListView lv, Broker broker)
        {

        }
        ///====================================================================
        /// Движение вверх (к корню) глобала
        ///====================================================================
        public void Up(ListView lv, Broker broker)
        {

        }
        ///====================================================================
    }
}
