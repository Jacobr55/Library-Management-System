namespace LibraryManagementSystem.Models
{
    public class ReturnBookViewModel
    {

        public int CheckoutID { get; set; }
        public string BookTitleName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime DueDate { get; set; }

        // Selected condition on form submit 
        public int ConditionID { get; set; }

        // Available conditions for the dropdown
        public List<SelectOption> Conditions { get; set; } = new();

    }
}
