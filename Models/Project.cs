using System.ComponentModel.DataAnnotations.Schema;

namespace IdmhProject.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public IFormFile ImageFile { get; set; }

        // Foreign Key for Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? TeamMember { get; set; } // Projenin ait olduğu takım üyesi
    }

}
