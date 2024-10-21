namespace IdmhProject.Models
{
    public class ContactFormSubmission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime SubmissionDate { get; set; }
    }

}
