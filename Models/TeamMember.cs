namespace IdmhProject.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }

        // Many-to-Many Relationship
        public ICollection<ProjectTeamMember> ProjectTeamMembers { get; set; }
    }

}
