namespace LibraryManagementSystem.Models
{
    public class PopularBookViewModel
    {
        public int Rank { get; set; }
        public int BookTitleID { get; set; }
        public string BookTitleName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string GenreName { get; set; } = "";
        public int CheckoutCount { get; set; }
        public int AvailableCopies { get; set; }

    }
}
