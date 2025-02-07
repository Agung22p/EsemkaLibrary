using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
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
        public event Action onBorrowed;
        private int memberId;
        public NewBorrowing(int memberID)
        {
            InitializeComponent();
            this.memberId = memberID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            load.SearchBook(textBox1, dataGridView1);
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0 && dgv.Columns.Contains("stock"))
            {
                int stock = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["stock"].Value);

                Color rowColor;

                if (stock == 0)
                {
                    rowColor = Color.Red;
                }
                else
                {
                    rowColor = Color.White;
                }

                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = rowColor;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if  (e.RowIndex >= 0 && e.ColumnIndex == dgv.Columns["btnBorrow"].Index)
            {
                int id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["id"].Value);

                string Title = dataGridView1.Rows[e.RowIndex].Cells["title"].Value.ToString();

                int dueDate = 7;
                MessageBox.Show($"Success Borrow '{Title}.' \n Due date is {dueDate} days from today.");

                load.BorrowBook(id, memberId);
                load.MinBookStock(Title);
                onBorrowed?.Invoke();
                this.Close();
            }
        }
    }
}
