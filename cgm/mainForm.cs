﻿using System;
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
            _rightbrocker = _leftbrocker;
        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripRight_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        //=====================================================================
        /// Изменение размера функциональных клавиш
        //=====================================================================
        private void toolStripBottom_Resize(object sender, EventArgs e)
        {
            //MessageBox.Show("hello");
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            //MessageBox.Show("hello");
            
        }
        //=====================================================================
    }
}
