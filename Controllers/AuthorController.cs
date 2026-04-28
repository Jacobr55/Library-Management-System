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
    }
}
