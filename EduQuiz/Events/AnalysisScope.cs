using EduQuiz.Models;
using EduQuiz.Services;
using Newtonsoft.Json;
using System.Text;

namespace EduQuiz.Events
{
    public class AnalysisScope
    {
        private readonly GeminiAiService _geminiaiService;

        // Constructor Injection for GeminiAiService
        public AnalysisScope(GeminiAiService geminiaiService)
        {
            _geminiaiService = geminiaiService;
        }
        public const string Instruction = @"
You are an expert in analyzing and classifying question sets into appropriate categories based on their content. Below are the requirements for categorizing the given question set into one relevant category:

### Task:
   - Analyze the given set of questions and classify it by selecting only **one** most relevant category that best describes the content of the questions.
   - Use the following list of categories:
     - [ { ""id"": 1, ""name"": ""Nghệ thuật"" }, { ""id"": 2, ""name"": ""Sinh học"" }, { ""id"": 3, ""name"": ""Kinh doanh"" }, { ""id"": 4, ""name"": ""Hóa học"" }, { ""id"": 5, ""name"": ""Tin tức hiện tại"" }, { ""id"": 6, ""name"": ""Kinh tế"" }, { ""id"": 7, ""name"": ""Tiếng Anh"" }, { ""id"": 8, ""name"": ""Giải trí"" }, { ""id"": 9, ""name"": ""Kiến thức tổng hợp"" }, { ""id"": 10, ""name"": ""Địa lý"" }, { ""id"": 11, ""name"": ""Lịch sử"" }, { ""id"": 12, ""name"": ""Ngôn ngữ"" }, { ""id"": 13, ""name"": ""Luật"" }, { ""id"": 14, ""name"": ""Toán học"" }, { ""id"": 15, ""name"": ""Âm nhạc"" }, { ""id"": 16, ""name"": ""Vật lý"" }, { ""id"": 17, ""name"": ""Chính trị"" }, { ""id"": 18, ""name"": ""Văn hóa phổ biến"" }, { ""id"": 19, ""name"": ""Tâm lý học"" }, { ""id"": 20, ""name"": ""Tôn giáo"" }, { ""id"": 21, ""name"": ""Khoa học"" }, { ""id"": 22, ""name"": ""Nghiên cứu xã hội"" }, { ""id"": 23, ""name"": ""Thể thao"" }, { ""id"": 24, ""name"": ""Công nghệ"" } ]

### Output Format:
   - Output should be a valid JSON array with a single object, in the format:
     ```json
     [{""id"": <category_id>, ""name"": ""<category_name>""}]
     ```
   - Do not provide explanations; only the ID and name of the selected category.

### Example Output:
```json
[{""id"": 10, ""name"": ""Địa lý""}]
```";
        public async Task<List<Analyze>> AnalyzeEduQuiz(EduQuizData getdatauizData)
        {
            try
            {
                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("## Your task:");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Analyze the following set of questions and classify it into a relevant category.");

                // Assuming `getdatauizData` contains the questions you want to analyze
                promptBuilder.AppendLine($"Questions: {JsonConvert.SerializeObject(getdatauizData)}");

                var response = await _geminiaiService.GenerateContent(Instruction, promptBuilder.ToString(), true, 40);
                return JsonConvert.DeserializeObject<List<Analyze>>(response);
            }
            catch
            {
                return new List<Analyze>();
            }
        }
    }
}
