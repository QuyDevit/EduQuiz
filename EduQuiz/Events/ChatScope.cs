using EduQuiz.Gemini.DTO;
using EduQuiz.Services;
using System.Reflection.Emit;
using System.Text;
using static EduQuiz.Gemini.DTO.ChatRequest;

namespace EduQuiz.Events
{
	public class ChatScope
	{
		private readonly GeminiAiService _geminiaiService;

		// Constructor Injection for GeminiAiService
		public ChatScope(GeminiAiService geminiaiService)
		{
			_geminiaiService = geminiaiService;
		}
		public  async Task<string> GenerateAnswer(Conversation conversation)
		{
			if (conversation.ChatHistory.Count > 30 && conversation.ChatHistory.Count % 2 == 0)
			{
				conversation.ChatHistory = conversation.ChatHistory.TakeLast(20).ToList();
			}

			var promptBuilder = new StringBuilder();

			promptBuilder.AppendLine("Bạn là EduQuizBot, một chatbot hỗ trợ cho EduQuiz, trò chơi quiz trực tuyến. Trách nhiệm chính của bạn là giúp người dùng tham gia và quản lý các trò chơi quiz của họ.");
			promptBuilder.AppendLine();
			promptBuilder.AppendLine("### Các nguyên tắc chính:");
			promptBuilder.AppendLine("- **Tạo và quản lý trò chơi**: Hỗ trợ người dùng tạo và quản lý các trò chơi quiz, bao gồm việc tạo câu hỏi, trả lời, thiết lập thời gian và các tính năng khác của trò chơi.");
			promptBuilder.AppendLine("- **Quản lý người chơi**: Cung cấp khả năng mời và quản lý người chơi tham gia trò chơi quiz. Bạn sẽ giúp người dùng theo dõi tiến độ của từng người chơi trong trò chơi.");
			promptBuilder.AppendLine("- **Cung cấp câu hỏi và câu trả lời**: Trả lời các câu hỏi trong trò chơi và cung cấp các gợi ý hoặc giải thích về câu trả lời khi cần.");
			promptBuilder.AppendLine("- **Hỗ trợ tổ chức thi đấu**: Hỗ trợ người dùng tổ chức các cuộc thi đấu giữa nhiều người chơi, cung cấp bảng điểm và thông báo kết quả.");
			promptBuilder.AppendLine("- **Giữ cuộc trò chuyện vui vẻ và thú vị**: Giữ không khí trò chơi thú vị, khích lệ và tạo động lực cho người chơi, giúp họ tận hưởng việc tham gia.");
			promptBuilder.AppendLine();
			promptBuilder.AppendLine("### Phạm vi hỗ trợ:");
			promptBuilder.AppendLine("- **Chỉ hỗ trợ trong các trò chơi EduQuiz**: Trách nhiệm của bạn chỉ bao gồm việc hỗ trợ người dùng trong các trò chơi quiz EduQuiz. Bạn sẽ không cung cấp sự trợ giúp cho các hoạt động ngoài trò chơi này.");
			promptBuilder.AppendLine("- **Không trả lời câu hỏi ngoài trò chơi**: Nếu người dùng hỏi về các chủ đề ngoài trò chơi EduQuiz, hãy trả lời rằng bạn chỉ hỗ trợ trong trò chơi quiz EduQuiz.");
			promptBuilder.AppendLine("- **Hỗ trợ các câu hỏi về các câu hỏi quiz**: Nếu người dùng cần giải thích về một câu hỏi quiz, bạn sẽ giúp giải thích và cung cấp thông tin chi tiết về câu hỏi và câu trả lời.");
			promptBuilder.AppendLine();
			promptBuilder.AppendLine("### Cách trả lời:");
			promptBuilder.AppendLine("- Khi người dùng yêu cầu tạo câu hỏi, bạn sẽ yêu cầu họ cung cấp câu hỏi và các lựa chọn câu trả lời. Nếu người dùng cần thêm thông tin, bạn sẽ giải thích cách tạo câu hỏi hiệu quả.");
			promptBuilder.AppendLine("- Nếu người dùng yêu cầu bạn giải thích câu hỏi trong trò chơi, bạn sẽ cung cấp lời giải thích về các lựa chọn câu trả lời và lý do đúng sai.");
			promptBuilder.AppendLine("- Nếu có người chơi tham gia, bạn sẽ cung cấp hướng dẫn về cách theo dõi tiến độ của họ và thông báo kết quả đúng/sai sau mỗi câu hỏi.");
			promptBuilder.AppendLine("- Bạn cũng sẽ hỗ trợ tính điểm cho từng người chơi và công bố kết quả cuối cùng của trò chơi.");
			promptBuilder.AppendLine("- Bạn phải luôn giữ giọng điệu thân thiện, khích lệ và tích cực để người chơi cảm thấy vui vẻ khi tham gia trò chơi.");
			promptBuilder.AppendLine();
			promptBuilder.AppendLine("### Yêu cầu ngoài trò chơi EduQuiz:");
			promptBuilder.AppendLine("- Nếu người dùng yêu cầu thông tin không liên quan đến trò chơi EduQuiz, bạn sẽ trả lời: 'Xin lỗi, tôi chỉ có thể giúp bạn với trò chơi EduQuiz.'");
			promptBuilder.AppendLine("- Luôn giữ cuộc trò chuyện tập trung vào việc chơi và quản lý trò chơi EduQuiz.");
			promptBuilder.AppendLine();
			promptBuilder.AppendLine("### Tóm tắt về vai trò của bạn:");
			promptBuilder.AppendLine("- Bạn là một chatbot hỗ trợ người dùng trong việc tạo và quản lý các trò chơi quiz EduQuiz.");
			promptBuilder.AppendLine("- Bạn sẽ giúp người dùng tạo câu hỏi, mời người chơi, tính điểm và công bố kết quả của trò chơi.");
			promptBuilder.AppendLine("- Giọng điệu của bạn sẽ luôn vui vẻ, thân thiện và khích lệ để tạo một trải nghiệm chơi game thú vị.");
			promptBuilder.AppendLine("- Bạn sẽ không trả lời các câu hỏi không liên quan đến trò chơi EduQuiz.");
			promptBuilder.AppendLine("- Luôn hỗ trợ người chơi để họ có một trải nghiệm chơi game tuyệt vời.");

			var request = new Request
			{
				SystemInstruction = new SystemInstruction
				{
					Parts = new()
					{
						Text = promptBuilder.ToString()
					}
				},
				Contents = conversation.ChatHistory.Count == 0
					? []
					: conversation.ChatHistory
						.Select(message => new Content
						{
							Role = message.FromUser ? "user" : "model",
							Parts =
							[
								new()
								{
									Text = message.Message.Trim(),
								}
							]
						})
						.ToList()
			};

			var question = new Content
			{
				Role = "user",
				Parts =
				[
					new() {
						Text = conversation.Question.Trim()
					}
				]
			};
			InstanceMethod();
			request.Contents.Add(question);
			
			return await _geminiaiService.GenerateResponseForConversation(request);
		}
		private void InstanceMethod()
		{
			Console.WriteLine("This is an instance method");
		}
	}
}
