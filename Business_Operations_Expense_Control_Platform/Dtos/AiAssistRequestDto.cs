using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class AiAssistRequestDto
    {
        [Required] public string Title { get; set; } = null!;
        public string? Reason { get; set; }
        [Required] public decimal Amount { get; set; }
    }

    public class AiAssistResponseDto
    {
        public string SuggestedTitle { get; set; } = null!;
        public string SuggestedReason { get; set; } = null!;
    }
}