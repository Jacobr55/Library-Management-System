namespace LibraryManagementSystem.Models
{
    public class BrowseBookViewModel
    {

        public int BookTitleID { get; set; }
        public string BookTitleName { get; set; } = "";
        public string ISBN { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string GenreName { get; set; } = "";
        public int AvailableCopies { get; set; }

    }
}
