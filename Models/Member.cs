namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public int MemberID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MemberName { get; set; }
        public int TotalBooksBorrowed { get; set; }

        public DateOnly? LastCheckoutDate { get; set; }

        // Convenience property
        public string FullName => $"{FirstName} {LastName}";
    }
}
