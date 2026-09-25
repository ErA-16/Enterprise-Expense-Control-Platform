using Business_Operations_Expense_Control_Platform.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public interface IAiService
    {
        Task<AiAssistResponseDto> GetExpenseAssistAsync(AiAssistRequestDto request);
    }
}
