namespace LibraryManagementSystem.Models
{
    public class MyCheckoutViewModel
    {

        public int CheckoutID { get; set; }
        public string BookTitleName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string ISBN { get; set; } = "";
        public DateTime CheckoutDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "";  // "Active", "Overdue", or "Lost"


    }
}
