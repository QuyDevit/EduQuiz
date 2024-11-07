using EduQuiz.Models;
using EduQuiz.Models.EF;

namespace EduQuiz.Areas.Admin.Models
{
    public class CollectionViewModel
    {
        public List<CollectionData> Collections { get; set; }
        public List<Music> Musics { get; set; }
        public List<Theme> Themes { get; set; }
    }
    public class CollectionEdit
    {
        public Collection Collection { get; set; }
        public List<EduQuizProfile> ListQuiz { get; set; }
    }
    public class CollectionData
    {
        public int Id { get; set; }
        public int SumActivity{ get; set; }
        public string Topic { get; set; }
        public string ImageCover { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Status { get; set; }
    }
}
