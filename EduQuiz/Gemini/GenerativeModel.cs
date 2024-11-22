using System.ComponentModel;

namespace EduQuiz.Gemini
{
    public enum GenerativeModel
    {
        [Description("gemini-1.0-pro")]
        Gemini_10_Pro = 1,

        [Description("gemini-1.5-pro-latest")]
        Gemini_15_Pro = 2,

        [Description("gemini-1.5-flash-latest")]
        Gemini_15_Flash = 3,
    }
}
