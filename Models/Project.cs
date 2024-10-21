namespace IdmhProject.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime CreatedDate { get; set; }

        // Foreign Key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Many-to-Many Relationship 
        public ICollection<ProjectTeamMember> ProjectTeamMembers { get; set; }
    }

}
