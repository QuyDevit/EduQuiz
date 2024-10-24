using EduQuiz.Models.EF;
using System.Collections.ObjectModel;

namespace EduQuiz.Models
{
    public class GroupViewModel
    {
        public Group Group { get; set; }
        public bool IsHost { get; set; }
        public string Avatar { get; set; }
        public List<MemberData> ListMember { get; set; }
        public List<EduQuizShared> ListEduQuizShared { get; set; }
        public List<EduQuizAssignment> ListEduQuiAssignment { get; set; }
        public ICollection<PostData> ListPost { get; set; }

    }
    public class GroupMemberView
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }
    public class GroupView
    {
        public Guid Uuid { get; set; }
        public string Name  { get; set; }
    }
    public class PostData
    {
        public int PostId { get; set; }
        public int CurrentUser { get; set; }
        public int UserCreate { get; set; }
        public string UserName { get; set; }
        public string PostedDate { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public int SumLike { get; set; }
        public bool IsLiked { get; set; }
    }
    public class MemberData
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }
        public string JoinDate { get; set; }
    }
    public class EduQuizShared
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Guid Uuid { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
        public int CountQuestion { get; set; }
    }
    public class EduQuizAssignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Guid Uuid { get; set; }
        public string Pin { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Avatar { get; set; }
        public string Username { get; set; }
        public string StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CountQuestion { get; set; }
    }

    public class ChallengeViewModel
    {
        public ChallengeModel Challenge { get; set; }
        public List<PlayerChallenge> ListPlayer { get; set; }
        public bool IsPlayer { get; set; }
        public int PlayerId { get; set; }
        public int QuestionChallengeCount { get; set; }

    }
    public class ChallengeModel
    {
        public int QuizSessionId { get; set; }
        public int GroupId { get; set; }
        public string Title { get; set; }
        public DateTime EndTime { get; set; }
        public string ImageCover { get; set; }
        public int QuestionCount { get; set; }
        public string UserCreate { get; set; }
    }
    public class PlayerChallenge
    {
        public int PlayerId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
    }
}
