using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    public class CatalogController : Controller
    {
        private readonly DatabaseHelper _db;

        public CatalogController(DatabaseHelper db) => _db = db;

        [HttpGet]
        public IActionResult Browse(int? genreId, int? authorId, string? search)
        {
            var viewModel = new BrowseBooksPageViewModel
            {
                SelectedGenreId = genreId,
                SelectedAuthorId = authorId,
                SearchTerm = search
            };

            using var conn = _db.GetConnection();
            conn.Open();

            // Load genre dropdown options
            using (var cmd = new SqlCommand(
                "SELECT GenreID, GenreName FROM Genre ORDER BY GenreName", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    viewModel.Genres.Add(new SelectOption
                    {
                        Id = (int)reader["GenreID"],
                        Name = reader["GenreName"].ToString()!
                    });
                }
            }

            // Load author dropdown options
            using (var cmd = new SqlCommand(
                "SELECT AuthorID, FirstName + ' ' + LastName AS FullName FROM Author ORDER BY LastName, FirstName", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    viewModel.Authors.Add(new SelectOption
                    {
                        Id = (int)reader["AuthorID"],
                        Name = reader["FullName"].ToString()!
                    });
                }
            }

            // Load books with optional filters
            string sql = @"
            SELECT
                bt.BookTitleID,
                bt.BookTitleName,
                bt.ISBN,
                a.FirstName + ' ' + a.LastName AS AuthorName,
                g.GenreName,
                (
                    SELECT COUNT(*)
                    FROM BookCopy bc
                    WHERE bc.BookTitleID = bt.BookTitleID
                      AND bc.RetiredDate IS NULL
                      AND NOT EXISTS (
                          SELECT 1 FROM Checkout c
                          WHERE c.BookCopyID = bc.BookCopyID
                            AND c.ReturnDate IS NULL
                      )
                ) AS AvailableCopies
            FROM BookTitle bt
            JOIN Author a ON a.AuthorID = bt.AuthorID
            JOIN Genre  g ON g.GenreID  = bt.GenreID
            WHERE (@GenreId  IS NULL OR bt.GenreID  = @GenreId)
              AND (@AuthorId IS NULL OR bt.AuthorID = @AuthorId)
              AND (@Search   IS NULL OR bt.BookTitleName LIKE '%' + @Search + '%')
            ORDER BY bt.BookTitleName";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@GenreId", (object?)genreId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AuthorId", (object?)authorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    viewModel.Books.Add(new BrowseBookViewModel
                    {
                        BookTitleID = (int)reader["BookTitleID"],
                        BookTitleName = reader["BookTitleName"].ToString()!,
                        ISBN = reader["ISBN"].ToString()!,
                        AuthorName = reader["AuthorName"].ToString()!,
                        GenreName = reader["GenreName"].ToString()!,
                        AvailableCopies = (int)reader["AvailableCopies"]
                    });
                }
            }

            return View(viewModel);
        }


        [HttpGet]
        [Authorize]
        public IActionResult MyCheckouts()
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(memberIdClaim, out int memberId))
            {
                return RedirectToAction("Login", "Account");
            }

            var checkouts = new List<MyCheckoutViewModel>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        SELECT
            c.CheckoutID,
            bt.BookTitleName,
            a.FirstName + ' ' + a.LastName AS AuthorName,
            bt.ISBN,
            c.CheckoutDate,
            c.DueDate,
            CASE
                WHEN c.DueDate < DATEADD(month, -6, GETDATE()) THEN 'Lost'
                WHEN c.DueDate < GETDATE() THEN 'Overdue'
                ELSE 'Active'
            END AS Status
        FROM Checkout c
        JOIN BookCopy  bc ON bc.BookCopyID  = c.BookCopyID
        JOIN BookTitle bt ON bt.BookTitleID = bc.BookTitleID
        JOIN Author    a  ON a.AuthorID     = bt.AuthorID
        WHERE c.MemberID = @MemberId
          AND c.ReturnDate IS NULL
        ORDER BY c.DueDate";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MemberId", memberId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                checkouts.Add(new MyCheckoutViewModel
                {
                    CheckoutID = (int)reader["CheckoutID"],
                    BookTitleName = reader["BookTitleName"].ToString()!,
                    AuthorName = reader["AuthorName"].ToString()!,
                    ISBN = reader["ISBN"].ToString()!,
                    CheckoutDate = (DateTime)reader["CheckoutDate"],
                    DueDate = (DateTime)reader["DueDate"],
                    Status = reader["Status"].ToString()!
                });
            }

            return View(checkouts);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(int bookTitleId)
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(memberIdClaim, out int memberId))
            {
                return RedirectToAction("Login", "Account");
            }

            using var conn = _db.GetConnection();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Find an available copy of this book title
                int? bookCopyId = null;

                string findSql = @"
            SELECT TOP 1 bc.BookCopyID
            FROM BookCopy bc
            WHERE bc.BookTitleID = @BookTitleID
              AND bc.RetiredDate IS NULL
              AND NOT EXISTS (
                  SELECT 1 FROM Checkout c
                  WHERE c.BookCopyID = bc.BookCopyID
                    AND c.ReturnDate IS NULL
              )";

                using (var findCmd = new SqlCommand(findSql, conn, transaction))
                {
                    findCmd.Parameters.AddWithValue("@BookTitleID", bookTitleId);
                    var result = findCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        bookCopyId = (int)result;
                    }
                }

                if (bookCopyId == null)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Sorry, no copies of that book are currently available.";
                    return RedirectToAction(nameof(Browse));
                }

                // Get the default "Good" condition ID
                int conditionId;
                using (var condCmd = new SqlCommand(
                    "SELECT ConditionID FROM Condition WHERE ConditionName = 'Good'", conn, transaction))
                {
                    var result = condCmd.ExecuteScalar();
                    if (result == null)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "System error: condition data is missing. Please contact a librarian.";
                        return RedirectToAction(nameof(Browse));
                    }
                    conditionId = (int)result;
                }

                // Create the checkout record
                string insertSql = @"
            INSERT INTO Checkout (BookCopyID, MemberID, ConditionID, CheckoutDate, DueDate)
            VALUES (@BookCopyID, @MemberID, @ConditionID, @CheckoutDate, @DueDate)";

                using (var insertCmd = new SqlCommand(insertSql, conn, transaction))
                {
                    insertCmd.Parameters.AddWithValue("@BookCopyID", bookCopyId.Value);
                    insertCmd.Parameters.AddWithValue("@MemberID", memberId);
                    insertCmd.Parameters.AddWithValue("@ConditionID", conditionId);
                    insertCmd.Parameters.AddWithValue("@CheckoutDate", DateTime.Today);
                    insertCmd.Parameters.AddWithValue("@DueDate", DateTime.Today.AddDays(14));
                    insertCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                TempData["Success"] = "Book checked out successfully! It's due in 14 days.";
                return RedirectToAction(nameof(MyCheckouts));
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult Return(int checkoutId)
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(memberIdClaim, out int memberId))
            {
                return RedirectToAction("Login", "Account");
            }
            using var conn = _db.GetConnection();
            conn.Open();
            // Load the checkout, verify it belongs to this member and isn't already returned
            var viewModel = new ReturnBookViewModel();
            bool found = false;
            string sql = @"
            SELECT
            c.CheckoutID,
            bt.BookTitleName,
            a.FirstName + ' ' + a.LastName AS AuthorName,
            c.DueDate
            FROM Checkout c
            JOIN BookCopy  bc ON bc.BookCopyID  = c.BookCopyID
            JOIN BookTitle bt ON bt.BookTitleID = bc.BookTitleID
            JOIN Author    a  ON a.AuthorID     = bt.AuthorID
            WHERE c.CheckoutID = @CheckoutID
            AND c.MemberID   = @MemberID
            AND c.ReturnDate IS NULL";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CheckoutID", checkoutId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    viewModel.CheckoutID = (int)reader["CheckoutID"];
                    viewModel.BookTitleName = reader["BookTitleName"].ToString()!;
                    viewModel.AuthorName = reader["AuthorName"].ToString()!;
                    viewModel.DueDate = (DateTime)reader["DueDate"];
                    found = true;
                }
            }
            if (!found)
            {
                TempData["Error"] = "That checkout was not found, or it has already been returned.";
                return RedirectToAction(nameof(MyCheckouts));
            }

            // Load condition options for the dropdown (all available conditions)
            using (var cmd = new SqlCommand(
                "SELECT ConditionID, ConditionName FROM Condition ORDER BY ConditionName", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    viewModel.Conditions.Add(new SelectOption
                    {
                        Id = (int)reader["ConditionID"],
                        Name = reader["ConditionName"].ToString()!
                    });
                }
            }

            // Default to "Good"
            var goodOption = viewModel.Conditions.FirstOrDefault(c => c.Name == "Good");
            if (goodOption != null) viewModel.ConditionID = goodOption.Id;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Return(ReturnBookViewModel model)
        {
            var memberIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(memberIdClaim, out int memberId))
            {
                return RedirectToAction("Login", "Account");
            }

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        UPDATE Checkout
        SET    ReturnDate  = @ReturnDate,
               ConditionID = @ConditionID
        WHERE  CheckoutID  = @CheckoutID
          AND  MemberID    = @MemberID
          AND  ReturnDate  IS NULL";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ReturnDate", DateTime.Today);
            cmd.Parameters.AddWithValue("@ConditionID", model.ConditionID);
            cmd.Parameters.AddWithValue("@CheckoutID", model.CheckoutID);
            cmd.Parameters.AddWithValue("@MemberID", memberId);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                TempData["Error"] = "That checkout could not be returned. It may have already been returned.";
            }
            else
            {
                TempData["Success"] = "Book returned successfully. Thank you!";
            }

            return RedirectToAction(nameof(MyCheckouts));


        }


        [HttpGet]
        public IActionResult Popular()
        {
            var books = new List<PopularBookViewModel>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        SELECT TOP 10
            bt.BookTitleID,
            bt.BookTitleName,
            a.FirstName + ' ' + a.LastName AS AuthorName,
            g.GenreName,
            COUNT(c.CheckoutID) AS CheckoutCount,
            (
                SELECT COUNT(*)
                FROM BookCopy bc2
                WHERE bc2.BookTitleID = bt.BookTitleID
                  AND bc2.RetiredDate IS NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM Checkout c2
                      WHERE c2.BookCopyID = bc2.BookCopyID
                        AND c2.ReturnDate IS NULL
                  )
            ) AS AvailableCopies
        FROM Checkout c
        JOIN BookCopy  bc ON bc.BookCopyID  = c.BookCopyID
        JOIN BookTitle bt ON bt.BookTitleID = bc.BookTitleID
        JOIN Author    a  ON a.AuthorID     = bt.AuthorID
        JOIN Genre     g  ON g.GenreID      = bt.GenreID
        WHERE MONTH(c.CheckoutDate) = MONTH(GETDATE())
          AND YEAR(c.CheckoutDate)  = YEAR(GETDATE())
        GROUP BY bt.BookTitleID, bt.BookTitleName, a.FirstName, a.LastName, g.GenreName
        ORDER BY CheckoutCount DESC, bt.BookTitleName";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            int rank = 1;
            while (reader.Read())
            {
                books.Add(new PopularBookViewModel
                {
                    Rank = rank++,
                    BookTitleID = (int)reader["BookTitleID"],
                    BookTitleName = reader["BookTitleName"].ToString()!,
                    AuthorName = reader["AuthorName"].ToString()!,
                    GenreName = reader["GenreName"].ToString()!,
                    CheckoutCount = (int)reader["CheckoutCount"],
                    AvailableCopies = (int)reader["AvailableCopies"]
                });
            }

            return View(books);
        }



    }
}
