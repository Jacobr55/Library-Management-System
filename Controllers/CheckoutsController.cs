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
    }
}
