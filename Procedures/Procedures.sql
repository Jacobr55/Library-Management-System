-- Aggregating Queries


-- 
-- Application Query 1#: Popular Genres This Month (Ranked)
-- Description: Returns all 12 genres ranked by checkout count
--              for the current calendar month. Includes genres with 0.
-- Invoked by: CatalogController.PopularGenres()
-- Parameters: (none — uses GETDATE())
--
-- Note: This is a different version from the Project Proposal.
-- I feel this query is more useful than just selecting the 1# genre checked out for the month.
-- I removed the Paramaters from my original query because I feel that they are not needed (Why not use GETDATE()?)  
--

SELECT
    g.GenreName,
    COUNT(c.CheckoutID) AS CheckoutCount
FROM Genre g
LEFT JOIN BookTitle bt ON bt.GenreID = g.GenreID
LEFT JOIN BookCopy  bc ON bc.BookTitleID = bt.BookTitleID
LEFT JOIN Checkout  c  ON c.BookCopyID = bc.BookCopyID
                      AND MONTH(c.CheckoutDate) = MONTH(GETDATE())
                      AND YEAR(c.CheckoutDate)  = YEAR(GETDATE())
GROUP BY g.GenreName
ORDER BY CheckoutCount DESC, g.GenreName;


/* 
Aggregating Query 2# – Inventory Status
Description: This will show how many physical books the library owns for each book title
and how many of them have been retired or removed.
Parameters: None
Result Columns:
BookTitle
TotalCopies
TotalRetiredCopies
*/

 SELECT bt.BookTitleID,
               bt.BookTitleName,
               COUNT(bc.BookCopyID)                              AS TotalCopies,
               SUM(CASE WHEN bc.RetiredDate IS NOT NULL THEN 1
                        ELSE 0 END)                             AS TotalRetiredCopies
        FROM   BookTitle bt
        LEFT JOIN BookCopy bc ON bt.BookTitleID = bc.BookTitleID
        GROUP  BY bt.BookTitleID, bt.BookTitleName
        ORDER  BY bt.BookTitleName;


/*
Aggregating Query 3# – Inactive Members
Description: This will find out if there are any inactive members who haven’t checked
out a book in a long time.
Parameters: InactiveDays – The number of days since the members has last checked
out a book.
Result Columns:
MemberID
Email
LastCheckoutDate


-- Note: @inactiveDays is supplied by the application at runtime via
-- MembersController code. To test manually in SSMS, declare it first:
--   DECLARE @inactiveDays INT = 365; 
*/

 SELECT m.MemberID,
               m.Email,
               MAX(c.CheckoutDate) AS LastCheckoutDate
        FROM   Members m
        LEFT JOIN Checkout c ON m.MemberID = c.MemberID
        GROUP  BY m.MemberID, m.Email
        HAVING MAX(c.CheckoutDate) < DATEADD(DAY, -@inactiveDays, GETDATE())
            OR MAX(c.CheckoutDate) IS NULL
        ORDER  BY LastCheckoutDate ASC;


/*
Aggregating Query 4# – Top Borrowers
Description: This will help find the library’s top 10 most loyal members, by checking to
see who has checked out the most books of all time.
Parameters: None
Result Columns:
MemberID
MemberName
TotalBooksBorrowed
*/

SELECT TOP 10
               m.MemberID,
               m.FirstName + ' ' + m.LastName AS MemberName,
               COUNT(c.CheckoutID) AS TotalBooksBorrowed
        FROM   Members m
        JOIN   Checkout c ON m.MemberID = c.MemberID
        GROUP  BY m.MemberID, m.FirstName, m.LastName
        ORDER  BY TotalBooksBorrowed DESC;

/* Aggregating Queries End */






/* Next, the application queries for the Librarian Access area (Views for the DB tables) */

/* NOTE: Some Operations use variables supplied by C# code, and thus, will not be executable IN SSMS */




/* ====================================================================
   AuthorsController — Librarian operations for the Author table
   ==================================================================== */


/*
Application Query: List All Authors
Description: Loads all authors ordered by last name for the librarian's
             author management page.
Invoked by: AuthorsController.Index()
Parameters: (none)
*/
SELECT AuthorID, FirstName, LastName, Bio
FROM   Author
ORDER  BY LastName;


/*
Application Query: Load Author for Edit
Description: Loads a single author's current values to populate the edit form.
Invoked by: AuthorsController.Edit(int id) — GET
Parameters: @id — the AuthorID being edited
*/
SELECT AuthorID, FirstName, LastName, Bio
FROM   Author
WHERE  AuthorID = @id;


/*
Application Query: Update Author
Description: Updates an existing author's information after the edit form
             is submitted.
Invoked by: AuthorsController.Edit(Author a) — POST
Parameters: @first, @last, @bio, @id
*/
UPDATE Author
SET    FirstName = @first,
       LastName  = @last,
       Bio       = @bio
WHERE  AuthorID  = @id;


/*
Application Query: Create Author
Description: Inserts a new author record from the create form.
Invoked by: AuthorsController.Create(Author a) — POST
Parameters: @first, @last, @bio
*/
INSERT INTO Author (FirstName, LastName, Bio)
VALUES (@first, @last, @bio);










/* ====================================================================
   BooksController — Librarian operations for the BookTitle table
   ==================================================================== */


/*
Application Query: List All Books
Description: Loads all book titles with their author names and genre names,
             ordered alphabetically by title. Used to display the librarian's
             book management page.
Invoked by: BooksController.Index()
Parameters: (none)
*/
SELECT bt.BookTitleID, bt.BookTitleName, bt.ISBN,
       a.FirstName + ' ' + a.LastName AS AuthorName,
       g.GenreName
FROM   BookTitle bt
JOIN   Author a ON bt.AuthorID = a.AuthorID
JOIN   Genre  g ON bt.GenreID  = g.GenreID
ORDER  BY bt.BookTitleName;


/*
Application Query: Load Book for Edit
Description: Loads a single book title's current values to populate the
             edit form, including its AuthorID and GenreID for dropdowns.
Invoked by: BooksController.Edit(int id) — GET
Parameters: @id — the BookTitleID being edited
*/
SELECT BookTitleID, BookTitleName, ISBN, AuthorID, GenreID
FROM   BookTitle
WHERE  BookTitleID = @id;


/*
Application Query: Update Book
Description: Updates an existing book title after the edit form is submitted.
Invoked by: BooksController.Edit(BookTitle b) — POST
Parameters: @name, @isbn, @authorId, @genreId, @id
*/
UPDATE BookTitle
SET    BookTitleName = @name,
       ISBN          = @isbn,
       AuthorID      = @authorId,
       GenreID       = @genreId
WHERE  BookTitleID   = @id;


/*
Application Query: Create Book
Description: Inserts a new book title from the create form.
Invoked by: BooksController.Create(BookTitle b) — POST
Parameters: @name, @isbn, @authorId, @genreId
*/
INSERT INTO BookTitle (BookTitleName, ISBN, AuthorID, GenreID)
VALUES (@name, @isbn, @authorId, @genreId);


/*
Application Query: Load Author Dropdown Options
Description: Loads all authors for the Author dropdown on the Create and
             Edit forms.
Invoked by: BooksController.LoadAuthors() — helper called by Edit GET and Create GET
Parameters: (none)
*/
SELECT AuthorID, FirstName, LastName
FROM   Author
ORDER  BY LastName;


/*
Application Query: Load Genre Dropdown Options
Description: Loads all genres for the Genre dropdown on the Create and
             Edit forms.
Invoked by: BooksController.LoadGenres() — helper called by Edit GET and Create GET
Parameters: (none)
*/
SELECT GenreID, GenreName
FROM   Genre
ORDER  BY GenreName;


/*
Application Query: Inventory Status
Description: Returns each book title with the total number of physical
             copies and the number of those that have been retired.
             This is the implementation of Aggregating Query #2.
Invoked by: BooksController.InventoryStatus()
Parameters: (none)
Note: Same query as Aggregating Query #2 below — shown here in its
      application-query context. See the Aggregating Queries section
      for the full description.
*/
SELECT bt.BookTitleID,
       bt.BookTitleName,
       COUNT(bc.BookCopyID)                              AS TotalCopies,
       SUM(CASE WHEN bc.RetiredDate IS NOT NULL THEN 1
                ELSE 0 END)                              AS TotalRetiredCopies
FROM   BookTitle bt
LEFT JOIN BookCopy bc ON bt.BookTitleID = bc.BookTitleID
GROUP  BY bt.BookTitleID, bt.BookTitleName
ORDER  BY bt.BookTitleName;










/* ====================================================================
   BookCopiesController — Librarian operations for the BookCopy table
   (Create / Read / Update — no hard delete - copies are retired instead)
   ==================================================================== */


/*
Application Query: List All Book Copies
Description: Loads all physical book copies with their parent book title,
             ordered alphabetically by title. Used to display the librarian's
             book copies management page.
Invoked by: BookCopiesController.Index()
Parameters: (none)
*/
SELECT bc.BookCopyID, bc.PurchasedDate, bc.RetiredDate,
       bt.BookTitleName
FROM   BookCopy bc
JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
ORDER  BY bt.BookTitleName;


/*
Application Query: Retire Book Copy
Description: Marks a book copy as retired by setting today's date as
             the RetiredDate. Used as a soft-delete alternative since
             retired copies still exist for historical checkout records.
Invoked by: BookCopiesController.Retire(int id) — POST
Parameters: @today — today's date, supplied by the application
            @id    — the BookCopyID being retired
*/
UPDATE BookCopy
SET    RetiredDate = @today
WHERE  BookCopyID  = @id;


/*
Application Query: Reactivate Book Copy
Description: Reactivates a previously retired copy by clearing the
             RetiredDate. Useful if a copy was retired by mistake or has
             returned to circulation.
Invoked by: BookCopiesController.Reactivate(int id) — POST
Parameters: @id — the BookCopyID being reactivated
*/
UPDATE BookCopy
SET    RetiredDate = NULL
WHERE  BookCopyID  = @id;


/*
Application Query: Create Book Copy
Description: Inserts a new physical copy of an existing book title with
             a purchase date. RetiredDate is left NULL by default, meaning
             the copy is active and available.
Invoked by: BookCopiesController.Create(BookCopy c) — POST
Parameters: @titleId   — the BookTitleID this copy belongs to
            @purchased — the date the copy was purchased
*/
INSERT INTO BookCopy (BookTitleID, PurchasedDate)
VALUES (@titleId, @purchased);


/*
Application Query: Load BookTitle Dropdown Options
Description: Loads all book titles for the BookTitle dropdown on the
             Create form.
Invoked by: BookCopiesController.LoadBookTitles() — helper called by Create GET
Parameters: (none)
*/
SELECT BookTitleID, BookTitleName
FROM   BookTitle
ORDER  BY BookTitleName;











/* ====================================================================
   AccountController — Member registration, login, and logout
   ==================================================================== */


/*
Application Query: Check Email Availability
Description: Counts existing members with the given email to verify
             uniqueness during registration. Used to provide a 
             error message before attempting the INSERT (the Email
             column also has a UNIQUE constraint as a safety net).
Invoked by: AccountController.Register(RegisterViewModel model) — POST
Parameters: @Email — the email address being registered
*/
SELECT COUNT(*)
FROM   Members
WHERE  Email = @Email;


/*
Application Query: Register New Member
Description: Inserts a new member record after registration. PasswordHash
             stores a one-way hash produced by Microsoft.AspNetCore.Identity's
             PasswordHasher (PBKDF2 with random salt). The original password
             is never stored.
Invoked by: AccountController.Register(RegisterViewModel model) — POST
Parameters: @Email, @First, @Last, @Hash
*/
INSERT INTO Members (Email, FirstName, LastName, PasswordHash)
VALUES (@Email, @First, @Last, @Hash);


/*
Application Query: Login Lookup
Description: Loads a member's MemberID, first name, and stored password
             hash by email for authentication. The application code then
             verifies the supplied password against the hash. If the email
             doesn't exist or PasswordHash is NULL (unregistered member),
             login is rejected with a generic "Invalid email or password"
             message to avoid leaking which emails are registered.
Invoked by: AccountController.Login(LoginViewModel model) — POST
Parameters: @Email — the email address attempting to log in
*/
SELECT MemberID, FirstName, PasswordHash
FROM   Members
WHERE  Email = @Email;










/* ====================================================================
   MembersController — Librarian operations the Members table
   (Read / Update only — no Create here; new members register via
    AccountController, and there's no hard delete.)
   ==================================================================== */


/*
Application Query: List All Members
Description: Loads all library members ordered by last name. Used to
             display the librarian's member management page.
Invoked by: MembersController.Index()
Parameters: (none)
*/
SELECT MemberID, FirstName, LastName, Email
FROM   Members
ORDER  BY LastName;


/*
Application Query: Load Member for Edit
Description: Loads a single member's current values to populate the
             edit form.
Invoked by: MembersController.Edit(int id) — GET
Parameters: @id — the MemberID being edited
*/
SELECT MemberID, FirstName, LastName, Email
FROM   Members
WHERE  MemberID = @id;


/*
Application Query: Update Member
Description: Updates an existing member's name and email after the
             librarian submits the edit form. PasswordHash is intentionally
             not modified here; password changes are out of scope for the
             librarian admin view.
Invoked by: MembersController.Edit(Member m) — POST
Parameters: @first, @last, @email, @id
*/
UPDATE Members
SET    FirstName = @first,
       LastName  = @last,
       Email     = @email
WHERE  MemberID  = @id;


/*
Application Query: Top Borrowers
Description: Returns the top 10 members by total checkout count of
             all time. This is the implementation of Aggregating Query #4.
Invoked by: MembersController.TopBorrowers()
Parameters: (none)
Note: Same query as Aggregating Query #4 below — shown here in its
      application-query context. See the Aggregating Queries section
      for the full description.
*/
SELECT TOP 10
       m.MemberID,
       m.FirstName + ' ' + m.LastName AS MemberName,
       COUNT(c.CheckoutID)             AS TotalBooksBorrowed
FROM   Members m
JOIN   Checkout c ON m.MemberID = c.MemberID
GROUP  BY m.MemberID, m.FirstName, m.LastName
ORDER  BY TotalBooksBorrowed DESC;


/*
Application Query: Inactive Members
Description: Returns members who have not checked out any book in the
             past @inactiveDays days, including members who have never
             checked anything out at all (LastCheckoutDate IS NULL).
             This is the implementation of Aggregating Query #3.
Invoked by: MembersController.InactiveMembers(int inactiveDays = 180)
Parameters: @inactiveDays — number of days since the member's last
                            checkout (defaults to 180 in C# if no value
                            is supplied via the query string)
Note: Same query as Aggregating Query #3 below — shown here in its
      application-query context. See the Aggregating Queries section
      for the full description.
*/
SELECT m.MemberID,
       m.Email,
       MAX(c.CheckoutDate) AS LastCheckoutDate
FROM   Members m
LEFT JOIN Checkout c ON m.MemberID = c.MemberID
GROUP  BY m.MemberID, m.Email
HAVING MAX(c.CheckoutDate) < DATEADD(DAY, -@inactiveDays, GETDATE())
    OR MAX(c.CheckoutDate) IS NULL
ORDER  BY LastCheckoutDate ASC;










/* ====================================================================
   CheckoutsController — Librarian Operations for the Checkout table
   (Read all checkouts, record returns)
   ==================================================================== */


/*
Application Query: List All Checkouts
Description: Loads all checkout records — past and present — with the
             book title, member name, and condition. Used by the
             librarian's checkout management page. Sorted by most recent
             checkout date first.
Invoked by: CheckoutsController.Index()
Parameters: (none)
*/
SELECT c.CheckoutID, c.CheckoutDate, c.DueDate, c.ReturnDate,
       bt.BookTitleName,
       m.FirstName + ' ' + m.LastName AS MemberName,
       co.ConditionName
FROM   Checkout c
JOIN   BookCopy  bc ON c.BookCopyID  = bc.BookCopyID
JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
JOIN   Members    m ON c.MemberID    = m.MemberID
JOIN   Condition co ON c.ConditionID = co.ConditionID
ORDER  BY c.CheckoutDate DESC;


/*
Application Query: Load Checkout for Return
Description: Loads a single checkout's full details to populate the
             librarian's return form, including book title, member name,
             and the existing return date if one is already set.
Invoked by: CheckoutsController.Return(int id) — GET
Parameters: @id — the CheckoutID being processed for return
*/
SELECT c.CheckoutID, c.BookCopyID, c.MemberID, c.ConditionID,
       c.CheckoutDate, c.DueDate, c.ReturnDate,
       bt.BookTitleName,
       m.FirstName + ' ' + m.LastName AS MemberName
FROM   Checkout c
JOIN   BookCopy  bc ON c.BookCopyID  = bc.BookCopyID
JOIN   BookTitle bt ON bc.BookTitleID = bt.BookTitleID
JOIN   Members    m ON c.MemberID    = m.MemberID
WHERE  c.CheckoutID = @id;


/*
Application Query: Record Return (Librarian)
Description: Records a book return by setting the ReturnDate and updating
             the ConditionID to the librarian's selected condition. Unlike
             the member-facing return flow, librarians may set a custom
             ReturnDate (defaults to today if not specified).
Invoked by: CheckoutsController.Return(Checkout c) — POST
Parameters: @returnDate, @conditionId, @id
*/
UPDATE Checkout
SET    ReturnDate  = @returnDate,
       ConditionID = @conditionId
WHERE  CheckoutID  = @id;


/*
Application Query: Load Condition Dropdown Options
Description: Loads all conditions for the Condition dropdown on the
             librarian's return form.
Invoked by: CheckoutsController.LoadConditions() — helper called by Return GET
Parameters: (none)
*/
SELECT ConditionID, ConditionName
FROM   Condition
ORDER  BY ConditionName;













/* ====================================================================
   CatalogController — Member catalog and self-service operations
   (Browse, Popular, Checkout, MyCheckouts, Return — public for browsing,
    [Authorize] required for checkout/return/my checkouts)
   ==================================================================== */


/*
Application Query: Load Genre Filter Options
Description: Loads all genres for the Genre filter dropdown on the Browse page.
Invoked by: CatalogController.Browse(...)
Parameters: (none)
*/
SELECT GenreID, GenreName
FROM   Genre
ORDER  BY GenreName;


/*
Application Query: Load Author Filter Options
Description: Loads all authors as "FirstName LastName" pairs for the
             Author filter dropdown on the Browse page.
Invoked by: CatalogController.Browse(...)
Parameters: (none)
*/
SELECT AuthorID, FirstName + ' ' + LastName AS FullName
FROM   Author
ORDER  BY LastName, FirstName;


/*
Application Query: Browse Books with Optional Filters
Description: Returns book titles with their author and genre names, plus a
             computed AvailableCopies count (copies that are not retired and
             not currently checked out). Supports optional filters by genre,
             author, and partial title search — any combination of which can
             be NULL to skip that filter.
Invoked by: CatalogController.Browse(int? genreId, int? authorId, string? search)
Parameters:
   @GenreId  — optional GenreID filter (NULL = no filter)
   @AuthorId — optional AuthorID filter (NULL = no filter)
   @Search   — optional partial-title search (NULL = no filter)
*/
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
ORDER BY bt.BookTitleName;


/*
Application Query: My Checkouts
Description: Returns the logged-in member's currently active (not yet
             returned) checkouts with a computed Status value:
               'Lost'    — DueDate is more than 6 months in the past
               'Overdue' — DueDate is in the past, less than 6 months
               'Active'  — DueDate is in the future
             The MemberID is sourced from the authentication cookie's
             claims, NOT from the URL or form data, so members can only
             see their own checkouts.
Invoked by: CatalogController.MyCheckouts()
Parameters: @MemberId — the logged-in member's MemberID, taken from the
                       NameIdentifier claim
*/
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
ORDER BY c.DueDate;


/*
Application Query: Find Available Copy
Description: Finds one available copy of the requested book title — meaning
             a copy that is not retired and is not currently checked out.
             The TOP 1 limits to a single result. Wrapped in a SqlTransaction
             with the subsequent INSERT to prevent a race condition where
             two members could grab the same last copy simultaneously.
Invoked by: CatalogController.Checkout(int bookTitleId) — POST
Parameters: @BookTitleID — the BookTitleID the member wants to check out
*/
SELECT TOP 1 bc.BookCopyID
FROM BookCopy bc
WHERE bc.BookTitleID = @BookTitleID
  AND bc.RetiredDate IS NULL
  AND NOT EXISTS (
      SELECT 1 FROM Checkout c
      WHERE c.BookCopyID = bc.BookCopyID
        AND c.ReturnDate IS NULL
  );


/*
Application Query: Lookup Default Condition (Good)
Description: Looks up the ConditionID for the "Good" condition by name.
             We don't hardcode the integer ID because lookup table IDs can
             change between environments — looking up by name keeps the
             code robust.
Invoked by: CatalogController.Checkout(int bookTitleId) — POST
Parameters: (none — name 'Good' is hardcoded in the SQL)
*/
SELECT ConditionID
FROM   Condition
WHERE  ConditionName = 'Good';


/*
Application Query: Create Checkout (Member self-service)
Description: Inserts a new checkout record for the logged-in member with
             a 14-day due date. CheckoutDate is set to today and ConditionID
             defaults to "Good" (looked up by name in the previous query).
             ReturnDate is left NULL by default — set when the book is
             returned.
Invoked by: CatalogController.Checkout(int bookTitleId) — POST
Parameters: @BookCopyID, @MemberID, @ConditionID, @CheckoutDate, @DueDate
*/
INSERT INTO Checkout (BookCopyID, MemberID, ConditionID, CheckoutDate, DueDate)
VALUES (@BookCopyID, @MemberID, @ConditionID, @CheckoutDate, @DueDate);


/*
Application Query: Load Checkout for Return (Member self-service)
Description: Loads a single checkout's details to populate the return
             confirmation page. Filters by both CheckoutID AND MemberID
             (from the auth cookie) so members can only see their own
             checkouts — even if they manually try a different ID in the URL,
             it won't return anything. Also filters out already-returned
             checkouts.
Invoked by: CatalogController.Return(int checkoutId) — GET
Parameters: @CheckoutID, @MemberID
*/
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
  AND c.ReturnDate IS NULL;


/*
Application Query: Load Condition Options for Return Form
Description: Loads all conditions for the dropdown on the member's return
             confirmation page.
Invoked by: CatalogController.Return(int checkoutId) — GET
Parameters: (none)
*/
SELECT ConditionID, ConditionName
FROM   Condition
ORDER  BY ConditionName;


/*
Application Query: Record Return (Member self-service)
Description: Records the member's return: sets ReturnDate to today and
             updates the ConditionID with the member-reported condition.
             Filters on MemberID (from auth cookie) so members can only
             return their own checkouts. Also filters on ReturnDate IS NULL
             so the same checkout can't be "returned" twice — a double-click
             or replayed form will simply update 0 rows. The application
             code checks rowsAffected to detect this and shows an
             appropriate message.
Invoked by: CatalogController.Return(ReturnBookViewModel model) — POST
Parameters: @ReturnDate, @ConditionID, @CheckoutID, @MemberID
*/
UPDATE Checkout
SET    ReturnDate  = @ReturnDate,
       ConditionID = @ConditionID
WHERE  CheckoutID  = @CheckoutID
  AND  MemberID    = @MemberID
  AND  ReturnDate  IS NULL;


/*
Application Query: Popular Books This Month
Description: Returns the top 10 most-checked-out book titles for the
             current calendar month. Joins down to Author and Genre for
             display, and includes a correlated subquery to compute the
             current AvailableCopies count for each book so users can
             check out directly from this page.
Invoked by: CatalogController.Popular()
Parameters: (none — uses GETDATE() for current month)
*/
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
ORDER BY CheckoutCount DESC, bt.BookTitleName;


/*
Application Query: Popular Genres This Month (Ranked)
Description: Returns all 12 genres ranked by checkout count for the
             current calendar month, including genres with zero checkouts
             (these come back with CheckoutCount = 0 thanks to LEFT JOINs).
             This is the implementation of Aggregating Query #1.
Invoked by: CatalogController.PopularGenres()
Parameters: (none — uses GETDATE())
Note: Same query as Aggregating Query #1 below — shown here in its
      application-query context. See the Aggregating Queries section
      for the full description.
*/
SELECT
    g.GenreName,
    COUNT(c.CheckoutID) AS CheckoutCount
FROM Genre g
LEFT JOIN BookTitle bt ON bt.GenreID = g.GenreID
LEFT JOIN BookCopy  bc ON bc.BookTitleID = bt.BookTitleID
LEFT JOIN Checkout  c  ON c.BookCopyID = bc.BookCopyID
                      AND MONTH(c.CheckoutDate) = MONTH(GETDATE())
                      AND YEAR(c.CheckoutDate)  = YEAR(GETDATE())
GROUP BY g.GenreName
ORDER BY CheckoutCount DESC, g.GenreName;



/* End of SQL Operations for the CIS-560 Library */




