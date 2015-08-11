using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Communication;

namespace cpm
{
    public partial class mainForm : Form
    {
        private Brocker _leftbrocker = null;
        private Brocker _rightbrocker = null;

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
            _leftbrocker = new Brocker();
            _rightbrocker = new Brocker();
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
            _serverL = "labc";
            _portL = "1972";
            _userL = "_system";
            _passwordL = "SYS";
            // правая панель
            _serverR = "labc-dev";
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
        private void PanelInit(Brocker brokerobj,ToolStripComboBox cbNSP, ListView lstview)
        {
            string nsp = brokerobj.GetNextNSP("");
            while (nsp != "")
            {                
                cbNSP.Items.Add(nsp);
                lstview.Items.Add(nsp);
                nsp = brokerobj.GetNextNSP(nsp);
            }
        }
        ///====================================================================
        /// <summary>
        /// Клик по элементу в левой панели
        /// </summary>
        ///====================================================================
        private void listViewLeft_Click(object sender, EventArgs e)
        {
            string nsp;
            if (listViewLeft.SelectedItems.Count == 0) return;
            nsp = listViewLeft.SelectedItems[0].Text;
            _leftbrocker.InitGlb(listViewLeft.SelectedItems[0].Text, _showsys);
        }
        ///====================================================================
        /// <summary>
        /// Клик по элементу в правой панели
        /// </summary>
        ///====================================================================
        private void listViewRight_Click(object sender, EventArgs e)
        {
            string nsp;
            if (listViewRight.SelectedItems.Count==0) return;
            nsp = listViewRight.SelectedItems[0].Text;
            _rightbrocker.InitGlb(nsp, _showsys);
        }
        ///====================================================================
    }
}
