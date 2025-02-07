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
        

        private readonly SqlConnection conn = new SqlConnection("Data Source=MSI\\SQLEXPRESS;Initial Catalog=EsemkaLibrary;Integrated Security=true;");

        private void ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
        }

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

        public int SearchMember(string Name)
        {
            object result = ExecuteScalar("SELECT id FROM Member WHERE name = @name;", new SqlParameter("@name", Name));
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void SearchBorrowBook(TextBox textBox, Button button, DataGridView dataGridView)
        {
            DataTable borrowedBook = ExecuteQuery(@"SELECT br.Id AS BorrowId, bk.title AS Title, br.borrow_date AS Borrow_Date, 
                                                        DATEADD(DAY, 7, br.borrow_date) AS Due_Date, 
                                                        CASE WHEN DATEDIFF(DAY, br.borrow_date, GETDATE()) > 7 
                                                             THEN DATEDIFF(DAY, br.borrow_date, GETDATE()) ELSE 0 
                                                        END AS Overdue_Days 
                                                    FROM Borrowing br 
                                                    JOIN Book bk ON br.book_id = bk.id 
                                                    JOIN Member m ON br.member_id = m.id 
                                                    WHERE m.name = @Name AND br.return_date IS NULL;",
                                                    new SqlParameter("@Name", textBox.Text));

            dataGridView.DataSource = borrowedBook;
            if (!dataGridView.Columns.Contains("btnReturn"))
            {
                dataGridView.Columns.Add(new DataGridViewLinkColumn
                {
                    Name = "btnReturn",
                    HeaderText = "Action",
                    Text = "Return",
                    UseColumnTextForLinkValue = true
                });
            }

            dataGridView.Columns["BorrowId"].Visible = false;
            button.Enabled = borrowedBook.Rows.Count < 3;
        }

        public void SearchBook(TextBox textBox, DataGridView dataGridView)
        {
            DataTable book = ExecuteQuery(@"SELECT 
                                            b.id, b.title, 
                                            STRING_AGG(g.name, ', ') AS Genre, 
                                            b.author, b.publish_date, b.stock 
                                        FROM BookGenre bg 
                                        JOIN Book b ON bg.book_id = b.id 
                                        JOIN Genre g ON bg.genre_id = g.id 
                                        WHERE b.title LIKE '%' + @title + '%' 
                                        GROUP BY b.id, b.title, b.author, b.publish_date, b.stock;",
                                        new SqlParameter("@title", textBox.Text));

            dataGridView.DataSource = book;
            if (!dataGridView.Columns.Contains("btnBorrow"))
            {
                dataGridView.Columns.Add(new DataGridViewLinkColumn
                {
                    Name = "btnBorrow",
                    HeaderText = "Action",
                    Text = "Borrow",
                    UseColumnTextForLinkValue = true
                });
            }

            dataGridView.Columns["id"].Visible = false;
        }

        public void UpdateReturn(int borrowId)
        {
            ExecuteNonQuery("UPDATE Borrowing SET return_date = @Return WHERE Id = @Id",
                new SqlParameter("@Id", borrowId),
                new SqlParameter("@Return", DateTime.Now));
        }

        public void UpdateBookStock(string bookTitle, int change)
        {
            ExecuteNonQuery("UPDATE Book SET stock = stock + @change WHERE title = @title",
                new SqlParameter("@title", bookTitle),
                new SqlParameter("@change", change));
        }

        public void AddBookStock(string bookTitle)
        {
            UpdateBookStock(bookTitle, 1);
        }

        public void MinBookStock(string bookTitle)
        {
            UpdateBookStock(bookTitle, -1);
        }

        public void BorrowBook(int bookId, int memberId)
        {
            ExecuteNonQuery("INSERT INTO Borrowing (member_id, book_id, borrow_date, created_at) VALUES (@memberId, @bookId, GETDATE(), GETDATE())",
                new SqlParameter("@memberId", memberId),
                new SqlParameter("@bookId", bookId));
            //SearchBorrowBook();
        }

    }
}
