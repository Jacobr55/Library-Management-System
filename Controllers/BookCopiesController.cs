using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    public class BookCopiesController : Controller
    {
        private readonly DatabaseHelper _db;

        public BookCopiesController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            var copies = new List<BookCopy>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT bc.BookCopyID, bc.PurchasedDate, bc.RetiredDate,
                   bt.BookTitleName
            FROM   BookCopy bc
            JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
            ORDER  BY bt.BookTitleName";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var copy = new BookCopy
                {
                    BookCopyID = (int)reader["BookCopyID"],
                    PurchasedDate = DateOnly.FromDateTime((DateTime)reader["PurchasedDate"]),
                    BookTitleName = reader["BookTitleName"].ToString()
                };

                if (reader["RetiredDate"] == DBNull.Value)
                {
                    copy.RetiredDate = null;
                }
                else
                {
                    copy.RetiredDate = DateOnly.FromDateTime((DateTime)reader["RetiredDate"]);
                }

                copies.Add(copy);
            }

            return View(copies);
        }
    }
}
