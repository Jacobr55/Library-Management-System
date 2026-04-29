using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    public class CheckoutsController : Controller
    {
        private readonly DatabaseHelper _db;

        public CheckoutsController(DatabaseHelper db) => _db = db;

        public IActionResult Index()
        {
            var checkouts = new List<Checkout>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT c.CheckoutID, c.CheckoutDate, c.DueDate, c.ReturnDate,
                   bt.BookTitleName,
                   m.FirstName + ' ' + m.LastName AS MemberName,
                   co.ConditionName
            FROM   Checkout c
            JOIN   BookCopy  bc ON c.BookCopyID  = bc.BookCopyID
            JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
            JOIN   Members    m ON c.MemberID    = m.MemberID
            JOIN   Condition co ON c.ConditionID = co.ConditionID
            ORDER  BY c.CheckoutDate DESC";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var checkout = new Checkout
                {
                    CheckoutID = (int)reader["CheckoutID"],
                    CheckoutDate = DateOnly.FromDateTime((DateTime)reader["CheckoutDate"]),
                    DueDate = DateOnly.FromDateTime((DateTime)reader["DueDate"]),
                    BookTitleName = reader["BookTitleName"].ToString(),
                    MemberName = reader["MemberName"].ToString(),
                    ConditionName = reader["ConditionName"].ToString()
                };

                if (reader["ReturnDate"] == DBNull.Value)
                {
                    checkout.ReturnDate = null;
                }
                else
                {
                    checkout.ReturnDate = DateOnly.FromDateTime((DateTime)reader["ReturnDate"]);
                }

                checkouts.Add(checkout);
            }

            return View(checkouts);
        }

        public IActionResult Return(int id)
        {
            Checkout? checkout = null;

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            SELECT c.CheckoutID, c.BookCopyID, c.MemberID, c.ConditionID,
                   c.CheckoutDate, c.DueDate, c.ReturnDate,
                   bt.BookTitleName,
                   m.FirstName + ' ' + m.LastName AS MemberName
            FROM   Checkout c
            JOIN   BookCopy  bc ON c.BookCopyID  = bc.BookCopyID
            JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
            JOIN   Members    m ON c.MemberID    = m.MemberID
            WHERE  c.CheckoutID = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    checkout = new Checkout
                    {
                        CheckoutID = (int)reader["CheckoutID"],
                        BookCopyID = (int)reader["BookCopyID"],
                        MemberID = (int)reader["MemberID"],
                        ConditionID = (int)reader["ConditionID"],
                        CheckoutDate = DateOnly.FromDateTime((DateTime)reader["CheckoutDate"]),
                        DueDate = DateOnly.FromDateTime((DateTime)reader["DueDate"]),
                        BookTitleName = reader["BookTitleName"].ToString(),
                        MemberName = reader["MemberName"].ToString()
                    };

                    if (reader["ReturnDate"] != DBNull.Value)
                    {
                        checkout.ReturnDate = DateOnly.FromDateTime((DateTime)reader["ReturnDate"]);
                    }
                }
            }

            if (checkout == null) return NotFound();

            ViewBag.Conditions = LoadConditions(conn);
            return View(checkout);
        }

        [HttpPost]
        public IActionResult Return(Checkout c)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
            UPDATE Checkout
            SET    ReturnDate  = @returnDate,
                   ConditionID = @conditionId
            WHERE  CheckoutID  = @id";

            var returnDate = c.ReturnDate ?? DateOnly.FromDateTime(DateTime.Today);

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@returnDate", returnDate.ToDateTime(TimeOnly.MinValue));
            cmd.Parameters.AddWithValue("@conditionId", c.ConditionID);
            cmd.Parameters.AddWithValue("@id", c.CheckoutID);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        private static List<Condition> LoadConditions(SqlConnection conn)
        {
            var list = new List<Condition>();
            using var cmd = new SqlCommand(
                "SELECT ConditionID, ConditionName FROM Condition ORDER BY ConditionName", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Condition
                {
                    ConditionID = (int)reader["ConditionID"],
                    ConditionName = reader["ConditionName"].ToString()!
                });
            }
            return list;
        }
    }
}
