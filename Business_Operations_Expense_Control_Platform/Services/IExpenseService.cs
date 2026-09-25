using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Microsoft.AspNetCore.Mvc;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public interface IExpenseService
    {
        Task<ExpenseRequestDisplayInfoDto> CreateExpenseAsync(CreateExpenseDto dto, int currentUserId);
        Task<List<ExpenseRequestDisplayInfoDto>> GetMyOwnRequestsAsync(int currentUserId);
        Task<List<ExpenseRequestDisplayInfoDto>> GetDepartmentRequestsAsync(int userDepartmentId);
        Task<bool> ProcessRequestAsync(int id, ManagerReviewExpenseDto reviewDto, int currentManagerId, int currentManagerDeptId);
        Task<List<ExpenseRequestDisplayInfoDto>> GetAprovedRequestsAsync();
        Task<bool> ProcessFinanceReviewAsync(int id, FinanceReviewExpenseDto reviewDto, int financeUserId);
        Task<List<ExpenseRequestDisplayInfoDto>> GetAllProcessingRequestsAsync();
        Task<bool> MakePaymentAsync(int id, MakePaymentDto dto, int financeUserId);
        Task<List<RequestAuditDisplayDto>> GetRequestHistoryAsync(int id, int currentUserId, string currentRole);
        Task<List<ExpenseByDepartmentDisplayDto>> GetExpenseReportByDepartmentAsync(int? managerDeptId);
        Task<List<ExpenseRequestDisplayInfoDto>> GetPaidRequestsAsync();
    }
}
