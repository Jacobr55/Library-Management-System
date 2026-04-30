namespace LibraryManagementSystem.Models
{
    public class BrowseBooksPageViewModel
    {
        public List<BrowseBookViewModel> Books { get; set; } = new();
        public List<SelectOption> Genres { get; set; } = new();
        public List<SelectOption> Authors { get; set; } = new();

        // Currently selected filters (so the page remembers what you filtered on)
        public int? SelectedGenreId { get; set; }
        public int? SelectedAuthorId { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class SelectOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}
