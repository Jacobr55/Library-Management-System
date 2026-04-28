namespace LibraryManagementSystem.Models
{
    public class Author
    {
        public int AuthorID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Bio { get; set; }

        // Convenience property for display
        public string FullName => $"{FirstName} {LastName}";
    }
}
