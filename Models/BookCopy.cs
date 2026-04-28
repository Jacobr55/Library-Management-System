namespace LibraryManagementSystem.Models
{
    public class BookCopy
    {
        public int BookCopyID { get; set; }
        public int BookTitleID { get; set; }
        public DateOnly PurchasedDate { get; set; }
        public DateOnly? RetiredDate { get; set; }

        // For joined display
        public string? BookTitleName { get; set; }
    }
}
