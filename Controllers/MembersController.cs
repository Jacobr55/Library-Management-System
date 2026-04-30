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

        public IActionResult Edit(int id)
        {
            Member? member = null;

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT MemberID, FirstName, LastName, Email
            FROM   Members
            WHERE  MemberID = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                member = new Member
                {
                    MemberID = (int)reader["MemberID"],
                    FirstName = reader["FirstName"].ToString()!,
                    LastName = reader["LastName"].ToString()!,
                    Email = reader["Email"].ToString()!
                };
            }

            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost]
        public IActionResult Edit(Member m)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            UPDATE Members
            SET    FirstName = @first,
                   LastName  = @last,
                   Email     = @email
            WHERE  MemberID  = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@first", m.FirstName);
            cmd.Parameters.AddWithValue("@last", m.LastName);
            cmd.Parameters.AddWithValue("@email", m.Email);
            cmd.Parameters.AddWithValue("@id", m.MemberID);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }


        public IActionResult TopBorrowers()
        {
            var topBorrowers = new List<Member>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        SELECT TOP 10
               m.MemberID,
               m.FirstName + ' ' + m.LastName AS MemberName,
               COUNT(c.CheckoutID) AS TotalBooksBorrowed
        FROM   Members m
        JOIN   Checkout c ON m.MemberID = c.MemberID
        GROUP  BY m.MemberID, m.FirstName, m.LastName
        ORDER  BY TotalBooksBorrowed DESC";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                topBorrowers.Add(new Member
                {
                    MemberID = (int)reader["MemberID"],
                    MemberName = reader["MemberName"].ToString()!,
                    TotalBooksBorrowed = (int)reader["TotalBooksBorrowed"]
                });
            }

            return View(topBorrowers);
        }

        public IActionResult InactiveMembers(int inactiveDays = 180)
        {
            var inactiveMembers = new List<Member>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        SELECT m.MemberID,
               m.Email,
               MAX(c.CheckoutDate) AS LastCheckoutDate
        FROM   Members m
        LEFT JOIN Checkout c ON m.MemberID = c.MemberID
        GROUP  BY m.MemberID, m.Email
        HAVING MAX(c.CheckoutDate) < DATEADD(DAY, -@inactiveDays, GETDATE())
            OR MAX(c.CheckoutDate) IS NULL
        ORDER  BY LastCheckoutDate ASC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inactiveDays", inactiveDays);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var member = new Member
                {
                    MemberID = (int)reader["MemberID"],
                    Email = reader["Email"].ToString()!
                };

                if (reader["LastCheckoutDate"] == DBNull.Value)
                {
                    member.LastCheckoutDate = null;
                }
                else
                {
                    member.LastCheckoutDate = DateOnly.FromDateTime((DateTime)reader["LastCheckoutDate"]);
                }

                inactiveMembers.Add(member);
            }

            ViewBag.InactiveDays = inactiveDays;
            return View(inactiveMembers);
        }
    }
}
