using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


public class BooksController : Controller
{
    private readonly DatabaseHelper _db;

    public BooksController(DatabaseHelper db) => _db = db;

    public IActionResult Index()
    {
        var books = new List<BookTitle>();

        using var conn = _db.GetConnection();
        conn.Open();

        string sql = @"
            SELECT bt.BookTitleID, bt.BookTitleName, bt.ISBN,
                   a.FirstName + ' ' + a.LastName AS AuthorName,
                   g.GenreName
            FROM   BookTitle bt
            JOIN   Author a ON bt.AuthorID = a.AuthorID
            JOIN   Genre  g ON bt.GenreID  = g.GenreID
            ORDER  BY bt.BookTitleName";

        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            books.Add(new BookTitle
            {
                BookTitleID = (int)reader["BookTitleID"],
                BookTitleName = reader["BookTitleName"].ToString()!,
                ISBN = reader["ISBN"].ToString()!,
                AuthorName = reader["AuthorName"].ToString(),
                GenreName = reader["GenreName"].ToString()
            });
        }

        return View(books);
    }

    public IActionResult Edit(int id)
    {
        BookTitle? book = null;

        using var conn = _db.GetConnection();
        conn.Open();

        string sql = @"
            SELECT BookTitleID, BookTitleName, ISBN, AuthorID, GenreID
            FROM   BookTitle
            WHERE  BookTitleID = @id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                book = new BookTitle
                {
                    BookTitleID = (int)reader["BookTitleID"],
                    BookTitleName = reader["BookTitleName"].ToString()!,
                    ISBN = reader["ISBN"].ToString()!,
                    AuthorID = (int)reader["AuthorID"],
                    GenreID = (int)reader["GenreID"]
                };
            }
        }

        if (book == null) return NotFound();

        ViewBag.Authors = LoadAuthors(conn);
        ViewBag.Genres = LoadGenres(conn);
        return View(book);
    }

    [HttpPost]
    public IActionResult Edit(BookTitle b)
    {
        using var conn = _db.GetConnection();
        conn.Open();

        string sql = @"
            UPDATE BookTitle
            SET    BookTitleName = @name,
                   ISBN          = @isbn,
                   AuthorID      = @authorId,
                   GenreID       = @genreId
            WHERE  BookTitleID   = @id";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", b.BookTitleName);
        cmd.Parameters.AddWithValue("@isbn", b.ISBN);
        cmd.Parameters.AddWithValue("@authorId", b.AuthorID);
        cmd.Parameters.AddWithValue("@genreId", b.GenreID);
        cmd.Parameters.AddWithValue("@id", b.BookTitleID);
        cmd.ExecuteNonQuery();

        return RedirectToAction(nameof(Index));
    }

    private static List<Author> LoadAuthors(SqlConnection conn)
    {
        var list = new List<Author>();
        using var cmd = new SqlCommand(
            "SELECT AuthorID, FirstName, LastName FROM Author ORDER BY LastName", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Author
            {
                AuthorID = (int)reader["AuthorID"],
                FirstName = reader["FirstName"].ToString()!,
                LastName = reader["LastName"].ToString()!
            });
        }
        return list;
    }

    private static List<Genre> LoadGenres(SqlConnection conn)
    {
        var list = new List<Genre>();
        using var cmd = new SqlCommand(
            "SELECT GenreID, GenreName FROM Genre ORDER BY GenreName", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Genre
            {
                GenreID = (int)reader["GenreID"],
                GenreName = reader["GenreName"].ToString()!
            });
        }
        return list;
    }
}