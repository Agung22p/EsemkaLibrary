using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EsemkaLibarary
{
    internal class LoadData
    {
        public SqlConnection conn = new SqlConnection("Data Source=MSI\\SQLEXPRESS;Initial Catalog=EsemkaLibrary;Integrated Security=true;");
        
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                conn.Open();
                dt.Load(cmd.ExecuteReader());
                conn.Close();
                return dt;
            }
        }

        public void SearchBorrowBook(TextBox textBox, Button button, DataGridView dataGridView)
        {
            
            DataTable borrowedBook = ExecuteQuery(@"SELECT br.Id AS BorrowId, bk.title AS Title, br.borrow_date AS Borrow_Date, 
                                                            DATEADD(DAY, 7,br.borrow_date) AS Due_Date, 
                                                            CASE
	                                                            WHEN DATEDIFF(DAY, br.borrow_date, GETDATE()) > 7 
                                                                THEN DATEDIFF(DAY, br.borrow_date, GETDATE()) 
	                                                            ELSE 0
                                                            END AS Overdue_Days 
                                                            FROM Borrowing br 
                                                            JOIN Book bk ON br.book_id = bk.id 
                                                            JOIN Member m ON br.member_id = m.id 
                                                        WHERE m.name = @Name AND br.return_date IS NULL;",
                                                        new SqlParameter("@Name", textBox.Text));

            

            DataGridViewLinkColumn btnReturn = new DataGridViewLinkColumn
            {
                Name = "btnReturn",
                HeaderText = "Action",
                Text = "Return",
                UseColumnTextForLinkValue = true
            };

            dataGridView.DataSource = borrowedBook;
            if (!dataGridView.Columns.Contains("btnReturn"))
            {
                dataGridView.Columns.Add(btnReturn);
            }

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].Tag = borrowedBook.Rows[i]["BorrowID"];
            }

            if (borrowedBook.Rows.Count < 3)
            {
                button.Enabled = true;
            }



            conn.Close();
        }

        public void SearchBook(TextBox textBox, DataGridView dataGridView)
        {
            DataTable book = ExecuteQuery(@"SELECT 
                                            b.title, 
                                            STRING_AGG(g.name, ', ') AS Genre, 
                                            b.author, 
                                            b.publish_date, 
                                            b.stock 
                                        FROM BookGenre bg 
                                        JOIN Book b ON bg.book_id = b.id 
                                        JOIN Genre g ON bg.genre_id = g.id 
                                        WHERE b.title LIKE '%"+textBox.Text+"%' " +
                                        "GROUP BY b.title, b.author, b.publish_date, b.stock;");
            DataGridViewLinkColumn linkBorrow = new DataGridViewLinkColumn
            {
                Name = "btnBorrow",
                HeaderText = "Action",
                Text = "Borrow",
                UseColumnTextForLinkValue = true
            };

            dataGridView.DataSource = book;
            if (!dataGridView.Columns.Contains("btnBorrow"))
            {
                dataGridView.Columns.Add(linkBorrow);
            }
        }

        public void UpdateReturn(int borrowId)
        {
            using (SqlCommand command = new SqlCommand("UPDATE Borrowing SET return_date = @Return WHERE Id = @Id", conn))
            {

                command.Parameters.AddWithValue("Id", borrowId);
                command.Parameters.AddWithValue("@Return", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
