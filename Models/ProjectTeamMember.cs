namespace IdmhProject.Models
{
    public class ProjectTeamMember
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int TeamMemberId { get; set; }
        public TeamMember TeamMember { get; set; }
    }

}
