using EduQuiz.Models.EF;

namespace EduQuiz.Models
{
    public class FolderViewModel
    {
        public ICollection<Folder> Folders { get; set; }
        public ICollection<Models.EF.EduQuiz> EduQuizs { get; set; }
    }
    public class EduQuizView
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Title { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public string Image { get; set; }
        public int SumQuestion { get; set; }
    }
}
