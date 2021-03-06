﻿using System;
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
        private Dictionary<ListView, Broker> brokers = null;
        private Dictionary<ListView, ToolStripComboBox> cbnsp = null;
        private Dictionary<ListView, ToolStripComboBox> cbservers = null;

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
            _showsys = "0";
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
            
            foreach (KeyValuePair<ListView,Broker> pair in brokers) 
            {
                if (pair.Value.Connect(_serverL, _portL, _userL, _passwordL))
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
            val = lv.SelectedItems[0].Text;
            if (".." == val)
            {
                brokerobj.Up(ShowSys);
            }
            else
            {
                brokerobj.Down(val);
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
            initDataBuf(lv, brocker, _showsys);

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
            //lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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
        }
        ///====================================================================
    }
}
