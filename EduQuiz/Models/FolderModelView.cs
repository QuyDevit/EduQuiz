using EduQuiz.Models.EF;

namespace EduQuiz.Models
{
    public class FolderModelView
    {
        public ICollection<Folder> Folders { get; set; }
        public ICollection<Models.EF.EduQuiz> EduQuizs { get; set; }
    }
}
