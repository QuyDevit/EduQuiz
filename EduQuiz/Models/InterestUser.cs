namespace EduQuiz.Models
{
    public class InterestUser
    {
        public int id { get; set; }
        public string name { get; set; }

    }
    public class HomeViewModel
    {
        public EF.User User { get; set; }
        public List<HomeReportView> ListReport { get; set; }
        public List<HomeAssignmentView> ListAssignment { get; set; }
        public List<HomeEduQuizView> ListEduQuiz { get; set; }
        public List<HomeEduQuizView> ListEduQuizOwner { get; set; }
        public List<HomeGroupView> ListGroupOwner { get; set; }
        public List<HomeGroupView> ListGroupJoin { get; set; }
    }
    public class HomeReportView
    {
        public int Id { get; set; }
        public string Pin { get; set; }
        public string ReportDate { get; set; }
        public string Title { get; set; }
    }
    public class HomeAssignmentView
    {
        public int Id { get; set; }
        public string Pin { get; set; }
        public string Deadline { get; set; }
        public string Image { get; set; }
        public string Title { get; set; }
        public int SumQuestion { get; set; }
        public string UserName { get; set; }
    }
    public class HomeEduQuizView
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Avatar { get; set; }
        public string UserName { get; set; }
        public int SumPlay { get; set; }
        public int SumQuestion { get; set; }
    }
    public class HomeGroupView
    {
        public int Id { get; set; }
        public bool IsHost { get; set; }
        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public int SumMember { get; set; }
    }
}
