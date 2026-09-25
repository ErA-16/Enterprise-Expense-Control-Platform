using Business_Operations_Expense_Control_Platform.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public interface IUserService
    {
        Task<List<UserDisplayDto>> GetAllEmployeesAsync(string? searchTerm, string? departmentFilter, int pageNumber, int pageSize);
        Task<UserDisplayDto> GetEmployeeByIdAsync(int id);
        Task<UserDisplayDto> PatchEmployeeDetailAsync(int id, PatchUserInfo dto);
        Task<bool> DeleteEmployeeAsync(int id);
    }
}
