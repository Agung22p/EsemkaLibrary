using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsemkaLibarary
{
    public partial class NewBorrowing : Form
    {
        LoadData load = new LoadData();
        public NewBorrowing()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            load.SearchBook(textBox1, dataGridView1);
        }
    }
}
