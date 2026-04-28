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
}