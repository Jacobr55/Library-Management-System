using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    public class MembersController : Controller
    {
        private readonly DatabaseHelper _db;

        public MembersController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            var members = new List<Member>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT MemberID, FirstName, LastName, Email 
            FROM   Members 
            ORDER  BY LastName";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                members.Add(new Member
                {
                    MemberID = (int)reader["MemberID"],
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!
                });
            }

            return View(members);
        }
    }
}
