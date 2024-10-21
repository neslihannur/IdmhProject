namespace IdmhProject.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishedDate { get; set; }

        // Foreign Key for Author
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }

}
