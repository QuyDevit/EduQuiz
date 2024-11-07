namespace EduQuiz.Models
{
    public class DiscoverViewModel
    {
        public List<CollectionDiscover> ListCollection { get; set; }
        public List<EduQuizItem> ListEduQuizRecommend { get; set; }
        public List<ProfileDiscover> ListProfile{ get; set; }
        public List<EduQuizItem> ListEduQuizHot{ get; set; }
    }
    public class CollectionDiscover
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string ImgCover { get; set; }
        public string UserName { get; set; }
        public int SumActive { get; set; }
        public string Avatar { get; set; }
    }
    public class ProfileDiscover
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string TitlePage { get; set; }
        public string ImgCover { get; set; }
        public string UserName { get; set; }
        public int SumEduQuiz { get; set; }
        public string Avatar { get; set; }
    }
}
