using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EduQuiz.Gemini;
using EduQuiz.Gemini.DTO;
using EduQuiz.Helper;
using EduQuiz.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;

namespace EduQuiz.Services
{
    public class GeminiAiService
    {
        private static readonly HttpClient Client = new();
        private readonly IConfiguration _configuration;
        public GeminiAiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GenerateContent(string? instruction, string query, bool useJson = true, double creativeLevel = 50, GenerativeModel model = GenerativeModel.Gemini_15_Flash)
        {
            var apiKey = _configuration["GoogleGenerativeAI:ApiKey"];
            var endpoint = GetUriWithHeadersIfAny(apiKey, model);

            var request = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = instruction
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = query
                            }
                        }
                    }
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" }
                },
                generationConfig = new
                {
                    temperature = creativeLevel / 100,
                    topP = 0.8,
                    topK = 40,
                    responseMimeType = useJson ? "application/json" :"text/plain"
                }
            };
            var body = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await Client.PostAsync(endpoint, body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseDTO = JsonConvert.DeserializeObject<ResponseForOneShot.Response>(responseData);

            return responseDTO.Candidates[0].Content.Parts[0].Text;
        }
		public async Task<string> GenerateResponseForConversation(ChatRequest.Request requestData)
		{
			var apiKey = _configuration["GoogleGenerativeAI:ApiKey"];
			var endpoint = GetUriWithHeadersIfAny(apiKey, GenerativeModel.Gemini_15_Flash);

			var body = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
			var response = await Client.PostAsync(endpoint, body);
			response.EnsureSuccessStatusCode();

			var responseData = await response.Content.ReadAsStringAsync();
			var dto = JsonConvert.DeserializeObject<ResponseForConversation.Response>(responseData);

			return dto.Candidates[0].Content.Parts[0].Text;
		}
		private static string GetUriWithHeadersIfAny(string accessKey, GenerativeModel model)
        {
            var modelName = GeneralHelper.GetEnumDescription(model);
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent";

            if (accessKey.StartsWith("AIza"))
            {
                endpoint += $"?key={accessKey}";
                return endpoint;
            }
            return endpoint;
        }
    }
}
