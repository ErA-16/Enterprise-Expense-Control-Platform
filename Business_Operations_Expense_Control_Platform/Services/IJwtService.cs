using Business_Operations_Expense_Control_Platform.Models;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
