using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace EsemkaLibarary
{
    public partial class EsemkaLibrary : Form
    {
        private int memberId;

        LoadData data = new LoadData();

        public EsemkaLibrary()
        {
            InitializeComponent();
        }

        private void EsemkaLibrary_Load(object sender, EventArgs e)
        {
            lblCurrentDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            data.SearchBorrowBook(textBox1, button2, dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int id = data.SearchMember(textBox1.Text);
            NewBorrowing newBorrowing = new NewBorrowing(id);

            newBorrowing.onBorrowed += RefreshDataGrid;

            newBorrowing.Show();
        }

        private void RefreshDataGrid()
        {
            data.SearchBorrowBook(textBox1,button2, dataGridView1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.ColumnIndex == dgv.Columns["btnReturn"].Index && e.RowIndex >= 0)
            {
                int borrowId = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["BorrowId"].Value);

                string TitleBook = dataGridView1.Rows[e.RowIndex].Cells["Title"].Value.ToString();
                int Overdue = (int)dataGridView1.Rows[e.RowIndex].Cells["Overdue_Days"].Value;
                string fineString = null;
                if (Overdue > 0)
                {
                    double totalfine = Overdue * 2000;
                    fineString = $"Member needs to pay fine : {totalfine} IDR";
                }

                MessageBox.Show($"Success return \"{TitleBook}.\" \n {fineString}", "Notification", MessageBoxButtons.OK);

                data.UpdateReturn(borrowId);
                data.AddBookStock(TitleBook);
                data.SearchBorrowBook(textBox1, button2, dataGridView1);
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.RowIndex >= 0 && dgv.Columns.Contains("Due_Date")) 
            {
                DateTime dueDate = Convert.ToDateTime(dgv.Rows[e.RowIndex].Cells["Due_Date"].Value);
                DateTime today = DateTime.Today;

                Color rowColor;

                if (dueDate < today) 
                {
                    rowColor = Color.Red; 
                }
                else if (dueDate == today) 
                {
                    rowColor = Color.Yellow; 
                }
                else 
                {
                    rowColor = Color.White;
                }

                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = rowColor;
                dgv.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
            }
        }
    }
}
