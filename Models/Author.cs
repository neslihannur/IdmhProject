namespace IdmhProject.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // One-to-Many Relationship 
        public ICollection<Blog> Blogs { get; set; }
    }

}
