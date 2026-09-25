using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public class AiService : IAiService
    {
        private readonly IChatClient _chatClient;

        public AiService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }
        
        public async Task<AiAssistResponseDto> GetExpenseAssistAsync(AiAssistRequestDto request)
        {
            string prompt = $$"""
                You are helping an employee write a clear, professional expense request.
                Given their draft below, rewrite the title and reason so they are clear,
                professional, and consistent with each other and the amount. Keep the
                same underlying request — do not invent a different item or purpose.
                Do not allow anything sexual at all oustisde of professional work ethics
                if anthign outside of that is in the request, remove it and write something like not allowed 
                Do not add your own extra spice and all that dont hallucinate and make up things that are
                not in the original request. Do not add any extra information that is not in the original request.
                Respond ONLY with valid JSON, nothing else, matching exactly this shape:
                {"suggestedTitle": string, "suggestedReason": string}

                Draft title: {{request.Title}}
                Draft reason: {{request.Reason}}
                Amount: {{request.Amount}}
                """;

            
            var messages = new[] { new ChatMessage(ChatRole.User, prompt) };
            var options = new ChatOptions { ResponseFormat = ChatResponseFormat.Json };

            var response = await _chatClient.GetResponseAsync(messages, options);

            AiAssistResponseDto? result;
            try
            {
                result = JsonSerializer.Deserialize<AiAssistResponseDto>(
                    response.Text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException)
            {
                throw new AiServiceException("AI response could not be parsed into the expected format.");
            }

            if (result == null)
            {
                throw new AiServiceException("AI service returned an unexpected response.");
            }

            return result;
        }
    }
}
