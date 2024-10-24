using System.ComponentModel.DataAnnotations.Schema;

namespace IdmhProject.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string? Image2 {  get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile> ImageFiles { get; set; }

        // Foreign Key for Category
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        // Foreign Key for ParentCategory
        
        public int? ParentCategoryId { get; set; } // Nullable yaparak isteğe bağlı hale getirebilirsiniz
        
        public ParentCategory? ParentCategory { get; set; } // İlişki

        public string? TeamMember { get; set; } // Projenin ait olduğu takım üyesi
        public string Content { get; set; }
    }

}
