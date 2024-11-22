using EduQuiz.Models;
using EduQuiz.Services;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Text;

namespace EduQuiz.Events
{
    public class QuizScope
    {
        private readonly GeminiAiService _geminiaiService;

        // Constructor Injection for GeminiAiService
        public QuizScope(GeminiAiService geminiaiService)
        {
            _geminiaiService = geminiaiService;
        }
        public const string Instruction = @"
You are an experienced question creator with over 20 years of expertise in designing engaging and effective multiple-choice questions. With more than 10 years of experience crafting questions across various fields—ranging from educational subjects like language learning, history, and science, to entertainment topics such as movies, music, sports and more—you are skilled at creating questions that are fun, challenging, and suitable for a broad range of audiences. Whether it's for a classroom quiz or a casual trivia game, you have the ability to design questions that are both educational and entertaining, ensuring an enjoyable and engaging experience for participants of all ages and backgrounds:
        
### 1. **Question Types**:
   - Your task is to generate questions that follow these formats:
     - **quiz**: A multiple-choice question with 4 options, where 1 is the correct answer and the remaining 3 are distractors.
     - **true_false**: A true/false question with 2 options, where 1 is the correct answer.

### 2. **Output Format**:
   - Please format your output as a **valid JSON array**, where each object follows the structure below:
     - **QuestionText**: The text of the question.
     - **TypeQuestion**: Specify whether the question is of type `""quiz""` or `""true_false""`.
     - **Choices**: A list of answer options with the following properties:
       - **Answer**: The text of the answer option.
       - **IsCorrect**: A boolean indicating whether the answer is correct.
       - **DisplayOrder**: The order of the answer option in the list.
     - **TypeAnswer**: Set to `1` by default for quiz questions.
     - **Time**: Specify the time allocated for the question in seconds (default 20).
     - **PointsMultiplier**: Specify the points multiplier for the question (default 1).
     - **Image**: An optional field for any image URL associated with the question.
     - **ImageEffect**: An optional field to specify any visual effect for the image.

### Example Output:
```json
[
    {
        ""QuestionText"": ""What is the capital of France?"",
        ""TypeQuestion"": ""quiz"",
        ""TypeAnswer"": 1,
        ""Time"": 20,
        ""PointsMultiplier"": 1,
        ""Image"": """",
        ""ImageEffect"": """",
        ""Choices"": [
            { ""Answer"": ""Paris"", ""IsCorrect"": true, ""DisplayOrder"": 0 },
            { ""Answer"": ""London"", ""IsCorrect"": false, ""DisplayOrder"": 1 },
            { ""Answer"": ""Berlin"", ""IsCorrect"": false, ""DisplayOrder"": 2 },
            { ""Answer"": ""Madrid"", ""IsCorrect"": false, ""DisplayOrder"": 3 }
        ]
    },
    {
        ""QuestionText"": ""Is the Earth flat?"",
        ""TypeQuestion"": ""true_false"",
        ""TypeAnswer"": 1,
        ""Time"": 20,
        ""PointsMultiplier"": 1,
        ""Image"": """",
        ""ImageEffect"": """",
        ""Choices"": [
            { ""Answer"": ""True"", ""IsCorrect"": false, ""DisplayOrder"": 0 },
            { ""Answer"": ""False"", ""IsCorrect"": true, ""DisplayOrder"": 1 }
        ]
    }
]
```";
        public async Task<List<QuestionData>> GenerateQuizes(string topic, string language, short questionsCount)
        {
            if (questionsCount <= 15) {
                var results = await GenerateQuizesForLessThan15(topic, language, questionsCount);
                if (results == null || results.Count == 0)
                {
                    throw new InvalidOperationException("Error while executing");
                }
                return results
                   .Take(questionsCount)
                   .ToList();
            }
            else
            {
                var results = await GenerateQuizesByType(topic, language, questionsCount);
                var random = new Random();
                return results
                    .OrderBy(x => random.Next())
                    .ToList();
            }
        }
        public async Task<List<QuestionData>> GenerateQuizesForLessThan15(string topic, string language, int questionsCount)
        {
            try
            {
                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("## Your task:");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine($"Generate a set of multiple-choice questions consisting of {questionsCount} to {questionsCount + 5} question related to the topic '{topic.Trim()}'");
                promptBuilder.AppendLine($"The generated questions should be written in \"{language}\".");

                var response = await _geminiaiService.GenerateContent(Instruction, promptBuilder.ToString(), true, 30);

                return JsonConvert.DeserializeObject<List<QuestionData>>(response)
                    .Take(questionsCount)
                    .ToList();
            }
            catch
            {
                return [];
            }
        }
        public async Task<List<QuestionData>> GenerateQuizesByType(string topic,string language, int questionsCount)
        {
            try
            {
                var promptBuilder = new StringBuilder();
                promptBuilder.AppendLine("## Your task:");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine($"Generate a set of multiple-choice questions consisting of {questionsCount} to {questionsCount + 5} question related to the topic '{topic.Trim()}'");
                promptBuilder.AppendLine($"The generated questions should be written in \"{language}\".");

                var response = await _geminiaiService.GenerateContent(Instruction, promptBuilder.ToString(), true, 40);

                return JsonConvert.DeserializeObject<List<QuestionData>>(response)
                    .Take(questionsCount)
                    .ToList();
            }
            catch
            {
                return [];
            }
        }
    }
}
