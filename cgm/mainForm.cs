﻿using System;
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
        private Dictionary<ListView, Adapter> adapters = null;
        private Dictionary<ListView, ToolStripComboBox> cbnsp = null;
        private Dictionary<ListView, ToolStripComboBox> cbservers = null;

        private string _showsys;
        // содержит элемент предыдущего уровня
        private string _prevPos;
        private int _lvsize;
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
            adapters = new Dictionary<ListView, Adapter>();
            cbnsp = new Dictionary<ListView, ToolStripComboBox>();
            cbservers = new Dictionary<ListView, ToolStripComboBox>();

            adapters.Add(listViewLeft, new Adapter());
            adapters.Add(listViewRight, new Adapter());

            cbnsp.Add(listViewLeft, toolStripLeftNSP);
            cbnsp.Add(listViewRight, toolStripRightNSP);

            cbservers.Add(listViewLeft, toolStripLeftServer);
            cbservers.Add(listViewRight, toolStripRightServer);

            _prevPos = "";
            _showsys = "1";
            _lvsize = 0;

        }
        ///====================================================================
        /// <summary>
        /// Прочитать настройки
        /// </summary>
        ///====================================================================

        private void readConfig()
        {
            // левая панель
            adapters[listViewLeft].Server = "localhost";
            adapters[listViewLeft].Port = "1972";
            adapters[listViewLeft].User = "_system";
            adapters[listViewLeft].Password = "SYS";
            // правая панель
            adapters[listViewRight].Server = "localhost";
            adapters[listViewRight].Port = "1972";
            adapters[listViewRight].User = "_system";
            adapters[listViewRight].Password = "SYS";
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
            
            foreach (KeyValuePair<ListView,Adapter> pair in adapters) 
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
            foreach (KeyValuePair<ListView, Adapter> pair in adapters)
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
        private void PanelInit(Adapter adapter,ToolStripComboBox cbNSP, ListView lstview)
        {
            string nsp = adapter.NextGlobal("");
            while (nsp != "")
            {                
                cbNSP.Items.Add(nsp);
                lstview.Items.Add(nsp);
                nsp = adapter.NextGlobal(nsp);
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
            Adapter br = adapters[lv];
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
            Adapter br = adapters[lv];
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
            Adapter br = adapters[lv];
            initDataBuf(lv, br, _showsys);
            ReLoad(lv, br);
        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера данных
        /// </summary>
        ///====================================================================
        private void initDataBuf(ListView lv, Adapter adapter, string ShowSys = "1")
        {
            string val;
            if (lv.SelectedItems.Count == 0) return;
            adapter.Start = "";
            adapter.DirectionSS = 1;
            adapter.Size = _lvsize;
            val = lv.SelectedItems[0].Text;
            if (".." == val)
            {
                _prevPos = adapter.Up(ShowSys);
            }
            else
            {
                adapter.Down(val, ShowSys);
            }

        }
        ///====================================================================
        /// <summary>
        /// Инициализация буфера данных
        /// </summary>
        ///====================================================================
        private void initDataBufOnLevel(ListView lv, Adapter adapter, string ShowSys = "1")
        {
            if (lv.SelectedItems.Count == 0) return;
            adapter.NarrUpDown(ShowSys);

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
        public void ReturnKeyDown(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            //MessageBox.Show("Enter Down");
            initDataBuf(lv, adapter, _showsys);

        }
        ///====================================================================
        /// <summary>
        /// Обработка отпускания клавиши Enter
        /// </summary>
        ///====================================================================
        public void ReturnKeyUp(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            ReLoad(lv, adapter);
        }
        ///====================================================================
        /// <summary>
        /// Стрелка вниз
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="adapter"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void DownKeyDown(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[lv.Items.Count-1].Selected)
            {
                adapter.Start = lv.Items[lv.Items.Count - 1].Text;
                adapter.Size = 2;
                adapter.DirectionSS = 1;
                initDataBufOnLevel(lv, adapter, _showsys);
                newitmtxt = adapter.NextGlobal(adapter.Start);
                if (newitmtxt !="")
                {
                    item=lv.Items.Add(newitmtxt);
                    RemoveFrom(lv,0);
                }

            }

        }
        ///====================================================================
        /// <summary>
        /// Стрелка вверх
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="adapter"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void UpKeyDown(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[0].Selected &&
                lv.Items[0].Text != "..")
            {
                adapter.Start = lv.Items[0].Text;
                adapter.Size = 2;
                adapter.DirectionSS = -1;
                initDataBufOnLevel(lv, adapter, _showsys);
                newitmtxt = adapter.PreviousGlobal(adapter.Start);
                if (newitmtxt != "")
                {
                    item = lv.Items.Insert(0, newitmtxt);
                    RemoveFrom(lv, lv.Items.Count - 1);
                }
                else
                {
                    if (adapter.Depth > 0)
                    {
                        item = lv.Items.Insert(0, "..");
                        RemoveFrom(lv, lv.Items.Count - 1);
                    }
                }

            }

        }
        ///====================================================================
        /// <summary>
        /// Кнопка Page Up
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="adapter"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void PageUpKeyDown(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[0].Selected &&
                lv.Items[0].Text != "..")
            {
                adapter.Start = lv.Items[0].Text;
                adapter.Size = _lvsize;
                adapter.DirectionSS = -1;
                initDataBufOnLevel(lv, adapter, _showsys);
                newitmtxt = adapter.PreviousGlobal(adapter.Start);
                if (newitmtxt != "")
                {
                    while (newitmtxt != "")
                    {
                        item = lv.Items.Insert(0, newitmtxt);
                        RemoveFrom(lv, lv.Items.Count - 1);
                        newitmtxt = adapter.PreviousGlobal(newitmtxt);
                    }
                }
                else
                {
                    if (adapter.Depth > 0)
                    {
                        item = lv.Items.Insert(0, "..");
                        RemoveFrom(lv, lv.Items.Count - 1);
                    }
                }

            }

        }
        ///====================================================================
        /// <summary>
        /// Кнопка Page Down
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="adapter"></param>
        /// <param name="evnt"></param>
        ///====================================================================
        public void NextKeyDown(ListView lv, Adapter adapter, KeyEventArgs evnt)
        {
            string newitmtxt;
            ListViewItem item = null;
            if (lv.Items[lv.Items.Count - 1].Selected)
            {
                adapter.Start = lv.Items[lv.Items.Count - 1].Text;
                adapter.Size = _lvsize;
                adapter.DirectionSS = 1;
                initDataBufOnLevel(lv, adapter, _showsys);
                newitmtxt = adapter.NextGlobal(adapter.Start);
                while (newitmtxt != "")
                {
                    item = lv.Items.Add(newitmtxt);
                    RemoveFrom(lv, 0);
                    newitmtxt = adapter.NextGlobal(newitmtxt);
                }

            }
        }
        ///====================================================================
            #endregion
            ///====================================================================
            /// <summary>
            /// Перечитать содержимое ListView
            /// </summary>
            ///====================================================================
        public void ReLoad(ListView lv, Adapter adapter)
        {
            string glb;
            lv.Items.Clear();
            glb = adapter.NextGlobal("");
            while (glb != "")
            {
                lv.Items.Add(glb);
                glb = adapter.NextGlobal(glb);
            }

            if (adapter.Depth > 0)
            {
                if ((adapter.Start == "") || 
                    (lv.Items.Count==0))
                {
                    lv.Items.Insert(0, "..");
                }
            }
            FocusLVItem(lv);            
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
            // Добавить перечитывание содержимого ListView
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
            _lvsize = (int)(lv.Height / 18);
            adapters[lv].Size = _lvsize;
        }
        ///====================================================================
        /// <summary>
        /// Удалить последний элемент из ListView
        /// </summary>
        ///====================================================================
        private void RemoveFrom(ListView lv, int pos=0)
        {
            if (lv.Items.Count > _lvsize)
            {
                lv.Items.RemoveAt(pos);
            }
        }
        ///====================================================================
    }
}
