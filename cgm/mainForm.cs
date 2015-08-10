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
        private string _serverL = "WIN10";
        private string _portL = "1972";
        private string _userL = "_system";
        private string _passwordL = "SYS";

        private string _serverR = "WIN10";
        private string _portR = "1972";
        private string _userR = "_system";
        private string _passwordR = "SYS";
        public mainForm()
        {
            InitializeComponent();
            _leftbrocker = new Brocker();
            _rightbrocker = new Brocker();
        }

        //=====================================================================
        /// Изменение размера функциональных клавиш
        //=====================================================================
        private void toolStripBottom_Resize(object sender, EventArgs e)
        {
            //MessageBox.Show("hello");
        }
        ///====================================================================
        /// Обработчик загрузки формы
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
        /// Закрытие формы
        ///====================================================================
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_leftbrocker.getConnectionStatus())
            {
                _leftbrocker.ClearNSP();
                _leftbrocker.Disconnect();
            }
            if (_rightbrocker.getConnectionStatus())
            {
                _rightbrocker.ClearNSP();
                _rightbrocker.Disconnect();
            }
        }
        ///====================================================================
        /// Обработчик события отрисовки контнейнера
        ///====================================================================
        private void splitContainer1_Paint(object sender, PaintEventArgs e)
        {            
            //PanelInit(_leftbrocker);
            //PanelInit(_rightbrocker);
            
        }
        ///====================================================================
        /// Инициализация панели
        ///====================================================================
        private void PanelInit(Brocker brokerobj,ToolStripComboBox cbNSP, ListView lstview)
        {
            //brokerobj.InitNSP();
            string nsp = brokerobj.GetNextNSP("");
            while (nsp != "")
            {                
                cbNSP.Items.Add(nsp);
                lstview.Items.Add(nsp);
                nsp = brokerobj.GetNextNSP(nsp);
            }

            //brokerobj.ClearNSP();
        }
        //=====================================================================
    }
}
