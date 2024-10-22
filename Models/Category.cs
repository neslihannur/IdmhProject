using System.ComponentModel.DataAnnotations;

namespace IdmhProject.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }  // Mimarlık, İç Mimarlık vb.

        public int? ParentCategoryId { get; set; }
        public ParentCategory ParentCategory { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
    public class ParentCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>(); // Alt kategoriler
    }

}
