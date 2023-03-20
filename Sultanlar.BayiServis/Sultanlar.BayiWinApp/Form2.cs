using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sultanlar.BayiWinApp
{
    public partial class Form2 : Form
    {
        public Form2(DataSet Ds)
        {
            InitializeComponent();
            ds = Ds;
        }

        DataSet ds;

        private void Form2_Load(object sender, EventArgs e)
        {
            dataGridView1.DataSource = ds.Tables[0];
            this.Text += " (Satır sayısı: " + ds.Tables[0].Rows.Count.ToString() + ")";
        }
    }
}
