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
            _leftbrocker.Connect("127.0.0.1", "1972");
            _rightbrocker.Connect("127.0.0.1", "1972");
            _leftbrocker.InitNSP();
            _rightbrocker.InitNSP();

            PanelInit(_leftbrocker,toolStripLeftNSP, listViewLeft);
            PanelInit(_rightbrocker,toolStripRightNSP, listViewRight);
        }
        ///====================================================================
        /// Закрытие формы
        ///====================================================================
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _leftbrocker.ClearNSP();
            _rightbrocker.ClearNSP();
            _leftbrocker.Disconnect();
            _rightbrocker.Disconnect();
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
