namespace IdmhProject.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }  // Mimarlık, İç Mimarlık vb.

        // One-to-Many Relationship with Projects
        public ICollection<Project> Projects { get; set; }
    }

}
