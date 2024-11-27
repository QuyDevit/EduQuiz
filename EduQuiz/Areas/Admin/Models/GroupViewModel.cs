namespace EduQuiz.Areas.Admin.Models
{
    public class GroupUserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SubscriptionType { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public DateTime CreateAt { get; set; }
        public int SumMember { get; set; }
        public bool Status { get; set; }
    }
}
