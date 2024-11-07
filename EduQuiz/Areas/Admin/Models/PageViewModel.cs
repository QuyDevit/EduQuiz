namespace EduQuiz.Areas.Admin.Models
{
    public class PageViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public string TitlePage { get; set; }
        public string DescriptionPage { get; set; }
        public DateTime CreateDate { get; set; }
        public bool Status { get; set; }
        public string ImagePage { get; set; }
        public Guid Uuid { get; set; }
    }
}
