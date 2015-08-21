using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
        private Dictionary<ListView, Broker> brokers = null;
        private Dictionary<ListView, ToolStripComboBox> cbnsp = null;
        private Dictionary<ListView, ToolStripComboBox> cbservers = null;

        private string _showsys;
        // содержит элемент предыдущего уровня
        private string _prevPos;
        ///====================================================================
        /// <summary>
        /// Конструктор
        /// </summary>
        ///====================================================================
        public mainForm()
        {
            InitializeComponent();
            MyInitializeComponent();
            readConfig();
        }
        ///====================================================================
        /// <summary>
        /// Инициализация "наших" компонентов
        /// </summary>
        ///====================================================================
        private void MyInitializeComponent()
        {
            brokers = new Dictionary<ListView, Broker>();
            cbnsp = new Dictionary<ListView, ToolStripComboBox>();
            cbservers = new Dictionary<ListView, ToolStripComboBox>();

            brokers.Add(listViewLeft, new Broker());
            brokers.Add(listViewRight, new Broker());

            cbnsp.Add(listViewLeft, toolStripLeftNSP);
            cbnsp.Add(listViewRight, toolStripRightNSP);

            cbservers.Add(listViewLeft, toolStripLeftServer);
            cbservers.Add(listViewRight, toolStripRightServer);

            _prevPos = "";
            _showsys = "1";

        }
        ///====================================================================
        /// <summary>
        /// Прочитать настройки
        /// </summary>
        ///====================================================================

        private void readConfig()
        {
            // левая панель
            brokers[listViewLeft].Server = "localhost";
            brokers[listViewLeft].Port = "1972";
            brokers[listViewLeft].User = "_system";
            brokers[listViewLeft].Password = "SYS";
            // правая панель
            brokers[listViewRight].Server = "localhost";
            brokers[listViewRight].Port = "1972";
            brokers[listViewRight].User = "_system";
            brokers[listViewRight].Password = "SYS";
            // Показать системные глобалы
            _showsys = "1";
        }

        ///====================================================================
        /// <summary>
        /// Обработчик загрузки формы
        /// </summary>
        ///====================================================================
        private void mainForm_Load(object sender, EventArgs e)
        {
            
            foreach (KeyValuePair<ListView,Broker> pair in brokers) 
            {
                if (pair.Value.Connect())
                {
                    pair.Value.InitNSP();
                    ToolStripComboBox toolStrip = cbnsp[pair.Key];
                    PanelInit(pair.Value, toolStrip, pair.Key);
                }
            }            
        }
        ///====================================================================
        /// <summary>
        /// Закрытие формы
        /// </summary>
        ///====================================================================
        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (KeyValuePair<ListView, Broker> pair in brokers)
            {
                if (pair.Value.ConnectionStatus)
                {
                    pair.Value.ClearBUF();
                    pair.Value.Disconnect();
                }
            }  
        }
        ///====================================================================
        /// <summary>
        /// Инициализация панели
        /// </summary>
        ///====================================================================
        private void PanelInit(Broker brokerobj,ToolStripComboBox cbNSP, ListView lstview)
        {
            string nsp = brokerobj.NextGlobal("");
            while (nsp != "")
            {                
                cbNSP.Items.Add(nsp);
                lstview.Items.Add(nsp);
                nsp = brokerobj.NextGlobal(nsp);
            }
            listView_SizeChanged(lstview);
        }
        ///====================================================================
        /// Блок кода отвечающий за обработку событий на ListView
        ///====================================================================
        #region Обработчики нажатия клавиш ListView
        ///====================================================================
        /// <summary>
        /// Нажатие клавиши на правой панели
        /// </summary>
        ///====================================================================
        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyCode.ToString());
            object[] args = null;
            ListView lv = (ListView)sender;
            Broker br = brokers[lv];
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyDown");
            if (null == method) return;
            args = new object[] { lv, br, e };
            method.Invoke(this, args);            
        }
        ///====================================================================
        /// <summary>
        /// Отпустили клавишу на правой панели
        /// </summary>
        ///====================================================================
        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            object[] args = null;
            ListView lv = (ListView)sender;
            Broker br = brokers[lv];
            MethodInfo method = this.GetType().GetMethod(e.KeyCode.ToString() + "KeyUp");
            if (null == method) return;
            args = new object[] { lv, br, e };
            method.Invoke(this, args);  
        }
        ///====================================================================
        /// <summary>
        /// Клик по элементу в левой панели
        /// </summary>
        ///====================================================================
        private void listView_Click(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;
            Broker br = brokers[lv];
            initDataBuf(lv, br, _showsys);
            ReLoad(lv, br);
        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера данных
        /// </summary>
        ///====================================================================
        private void initDataBuf(ListView lv, Broker brokerobj, string ShowSys = "1")
        {
            string val;
            if (lv.SelectedItems.Count == 0) return;
            brokerobj.Start = "";
            brokerobj.DirectionSS = 1;
            CalcAndSetSize(lv);
            val = lv.SelectedItems[0].Text;
            if (".." == val)
            {
                _prevPos = brokerobj.Up(ShowSys);
            }
            else
            {
                brokerobj.Down(val, ShowSys);
            }

        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера данных
        /// </summary>
        ///====================================================================
        private void initDataBufOnLevel(ListView lv, Broker brokerobj, string ShowSys = "1")
        {
            if (lv.SelectedItems.Count == 0) return;
            if (lv.Items[0].Selected)
            {
                brokerobj.NarrUp(ShowSys);
            }
            else
            {
                brokerobj.NarrDown(ShowSys);
            }

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
            brocker.Start = "";
            brocker.DirectionSS = 1;
            CalcAndSetSize(lv);
            initDataBuf(lv, brocker, _showsys);

        }
        ///====================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="brocker"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void DownKeyDown(ListView lv, Broker brocker, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[lv.Items.Count-1].Selected)
            {
                brocker.Start = lv.Items[lv.Items.Count - 1].Text;
                brocker.Size = 2;
                brocker.DirectionSS = 1;
                initDataBufOnLevel(lv, brocker, _showsys);
                newitmtxt = brocker.NextGlobal(brocker.Start);
                if (newitmtxt !="")
                {
                    item=lv.Items.Add(newitmtxt);
                    lv.Items.RemoveAt(0);
                }

            }

        }
        ///====================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="broker"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void UpKeyDown(ListView lv, Broker broker, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[0].Selected &&
                lv.Items[0].Text != "..")
            {
                broker.Start = lv.Items[0].Text;
                broker.Size = 2;
                broker.DirectionSS = -1;
                initDataBufOnLevel(lv, broker, _showsys);
                newitmtxt = broker.PreviousGlobal(broker.Start);
                if (newitmtxt != "")
                {
                    item = lv.Items.Insert(0, newitmtxt);
                    lv.Items.RemoveAt(lv.Items.Count-1);
                }
                else
                {
                    if (null != broker.NameSpace)
                    {
                        item = lv.Items.Insert(0, "..");
                        lv.Items.RemoveAt(lv.Items.Count - 1);
                    }
                }

            }

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
        /// Перечитать содержимое ListView
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
            FocusLVItem(lv);
            //lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        ///====================================================================
        /// <summary>
        /// Выделить элемент, на котором стояли при переходе в глубь дерева
        /// </summary>
        ///====================================================================
        private void FocusLVItem(ListView lv)
        {
            lv.Focus();
            if ("" != _prevPos)
            {
                ListViewItem item = lv.FindItemWithText(_prevPos);
                if (item != null)
                {
                    item.Selected = true;
                    lv.FocusedItem=item;
                    lv.TopItem = item;
                }
                else
                {
                    lv.Items[0].Selected = true;
                    lv.FocusedItem=lv.Items[0];
                    lv.TopItem = lv.Items[0];
                }
            }
            else
            {
                lv.Items[0].Selected = true;
                lv.FocusedItem=lv.Items[0];
                lv.TopItem = lv.Items[0];
            }            
        }
        ///====================================================================
        /// <summary>
        /// Автоматическое изменение размера колонок с сохранение пропроций
        /// (метод для левой и правой панели)
        /// </summary>
        ///====================================================================
        private void listView_SizeChanged(object sender, EventArgs e = null)
        {
            //return;        
            double prc;
            bool first = true;
            ListView lv = (ListView)sender;
            prc = 0.4 / (lv.Columns.Count - 1);
            foreach (ColumnHeader header in lv.Columns)
            {
                if (first)
                {
                    header.Width = (int)(lv.Width * 0.6);
                    first = !first;
                }
                else
                {
                    header.Width = (int)(lv.Width * prc);
                }
            }
            CalcAndSetSize(lv);

        }
        ///====================================================================
        /// <summary>
        /// Изменение ширины функциональных клавиш при изменении размера окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///====================================================================
        private void toolStripBottom_SizeChanged(object sender, EventArgs e)
        {
            int sepr = 6 * 11; // сумма ширины всех разделителей
            int wdthKeys = (toolStripBottom.Width - sepr);
            foreach (ToolStripItem itm in toolStripBottom.Items)
            {
                if (itm.GetType() == typeof(ToolStripButton)) 
                {
                    itm.Width = (int) (wdthKeys / 12);
                }
            }
 
        }
        ///====================================================================
        /// <summary>
        /// Установить размер буфера
        /// </summary>
        ///====================================================================
        private void CalcAndSetSize(ListView lv)
        {
            int sz;
            sz = (int)(lv.Height / 18);
            brokers[lv].Size = sz;
        }
        ///====================================================================
    }
}
