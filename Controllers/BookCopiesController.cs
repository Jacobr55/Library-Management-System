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

        [HttpPost]
        public IActionResult Retire(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            UPDATE BookCopy
            SET    RetiredDate = @today
            WHERE  BookCopyID  = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@today", DateTime.Today);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Reactivate(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            UPDATE BookCopy
            SET    RetiredDate = NULL
            WHERE  BookCopyID  = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            using var conn = _db.GetConnection();
            conn.Open();
            ViewBag.BookTitles = LoadBookTitles(conn);
            return View();
        }

        [HttpPost]
        public IActionResult Create(BookCopy c)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            INSERT INTO BookCopy (BookTitleID, PurchasedDate)
            VALUES (@titleId, @purchased)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@titleId", c.BookTitleID);
            cmd.Parameters.AddWithValue("@purchased", c.PurchasedDate.ToDateTime(TimeOnly.MinValue));
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        private static List<BookTitle> LoadBookTitles(SqlConnection conn)
        {
            var list = new List<BookTitle>();
            using var cmd = new SqlCommand(
                "SELECT BookTitleID, BookTitleName FROM BookTitle ORDER BY BookTitleName", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BookTitle
                {
                    BookTitleID = (int)reader["BookTitleID"],
                    BookTitleName = reader["BookTitleName"].ToString()!
                });
            }
            return list;
        }
    }
}
