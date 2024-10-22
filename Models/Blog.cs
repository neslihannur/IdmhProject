using System.ComponentModel.DataAnnotations.Schema;

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
        public string Image { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile> ImageFiles { get; set; }
    }

}
