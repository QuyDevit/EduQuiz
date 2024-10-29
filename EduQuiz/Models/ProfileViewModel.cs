﻿using EduQuiz.Models.EF;

namespace EduQuiz.Models
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public Profile Profile { get; set; }
        public List<EduQuizProfile> ListEduQuizProfile { get; set; }
    }
    public class EduQuizProfile
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public int Id { get; set; }
    }
    public class ProfilePage
    {
        public string PageTitle { get; set; }
        public string ImageCover { get; set; }
        public string PageDescription { get; set; }
        public string Avatar { get; set; }
        public int SumEduQuiz { get; set; }
        public int SumPlay { get; set; }
        public int SumPlayerPlay { get; set; }
        public string Email { get; set; }
        public string LinkZalo { get; set; }
        public string LinkYoutube { get; set; }
        public string LinkFacebook { get; set; }
        public string LinkInstagram { get; set; }
        public bool IsFollow {  get; set; }
        public bool IsHost { get; set; }
        public List<EduQuizItem> ListEduQuizItem { get; set; }
    }
    public class EduQuizItem
    {
        public string Title { get; set; }
        public Guid Uuid { get; set; }
        public string Type { get; set; }
        public int SumQuestion { get; set; }
        public string Image { get; set; }
        public string UserName { get; set; }
    }
}
