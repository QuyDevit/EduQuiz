using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EduQuiz.Models;
using Microsoft.Extensions.Configuration;

namespace EduQuiz.Services
{
    public class GeminiAiService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        public GeminiAiService(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async Task<string> GenerateResponse(ChatRequest request)
        {
            var client = _clientFactory.CreateClient();
            var apiKey = _configuration["GoogleGenerativeAI:ApiKey"];
            int maxTokens = request.MaxTokens <= 0 ? 2048 : request.MaxTokens;

            var geminiRequest = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = request.Messages[0].Content
                        }
                    }
                }
            },
                generationConfig = new
                {
                    maxOutputTokens = maxTokens,
                    temperature = 0.7,
                    topP = 0.8,
                    topK = 40
                }
            };

            var requestUrl = $"{GEMINI_API_URL}?key={apiKey}";
            var requestContent = new StringContent(
                JsonSerializer.Serialize(geminiRequest),
                Encoding.UTF8,
                "application/json"
            );

            var geminiResponse = await client.PostAsync(requestUrl, requestContent);
            var responseContent = await geminiResponse.Content.ReadAsStringAsync();

            if (!geminiResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API error: {responseContent}");
            }

            return responseContent;
        }
    }
}
