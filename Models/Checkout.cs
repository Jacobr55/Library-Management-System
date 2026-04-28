namespace LibraryManagementSystem.Models
{
    public class Checkout
    {
        public int CheckoutID { get; set; }
        public int BookCopyID { get; set; }
        public int MemberID { get; set; }
        public int ConditionID { get; set; }
        public DateOnly CheckoutDate { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }

        // For joined display
        public string? BookTitleName { get; set; }
        public string? MemberName { get; set; }
        public string? ConditionName { get; set; }
    }
}
