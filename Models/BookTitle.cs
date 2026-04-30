namespace LibraryManagementSystem.Models
{

    public class BookTitle
    {
        public int BookTitleID { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public int AuthorID { get; set; }
        public int GenreID { get; set; }
        public string BookTitleName { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int TotalRetiredCopies { get; set; }

        // For joined display
        public string? AuthorName { get; set; }
        public string? GenreName { get; set; }
    }
}
