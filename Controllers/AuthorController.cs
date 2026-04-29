using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly DatabaseHelper _db;

        public AuthorsController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            var authors = new List<Author>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT AuthorID, FirstName, LastName, Bio 
            FROM   Author 
            ORDER  BY LastName";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                authors.Add(new Author
                {
                    AuthorID = (int)reader["AuthorID"],
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Bio = reader["Bio"] as string
                });
            }

            return View(authors);
        }

        public IActionResult Edit(int id)
        {
            Author? author = null;

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT AuthorID, FirstName, LastName, Bio
            FROM   Author
            WHERE  AuthorID = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                author = new Author
                {
                    AuthorID = (int)reader["AuthorID"],
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Bio = reader["Bio"] as string
                };
            }

            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost]
        public IActionResult Edit(Author a)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            UPDATE Author
            SET    FirstName = @first,
                   LastName  = @last,
                   Bio       = @bio
            WHERE  AuthorID  = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@first", a.FirstName);
            cmd.Parameters.AddWithValue("@last", a.LastName);
            cmd.Parameters.AddWithValue("@bio", (object?)a.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", a.AuthorID);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }
    }
}
