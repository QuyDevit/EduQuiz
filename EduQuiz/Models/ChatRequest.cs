using EduQuiz.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EduQuiz.Models
{
    public class ChatRequest
    {
        public string Model { get; set; }
        public int MaxTokens { get; set; }
        public Message[] Messages { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
    public class GeminiResponse
    {
        public Candidate[] Candidates { get; set; }
        public UsageMetadata UsageMetadata { get; set; }
        public string ModelVersion { get; set; }
    }

    public class Candidate
    {
        public Content Content { get; set; }
        public string FinishReason { get; set; }
        public int Index { get; set; }
        public SafetyRating[] SafetyRatings { get; set; }
    }

    public class Content
    {
        public Part[] Parts { get; set; }
        public string Role { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
    }

    public class UsageMetadata
    {
        public int PromptTokenCount { get; set; }
        public int CandidatesTokenCount { get; set; }
        public int TotalTokenCount { get; set; }
    }

    public class SafetyRating
    {
        public string Category { get; set; }
        public string Probability { get; set; }
    }
}
