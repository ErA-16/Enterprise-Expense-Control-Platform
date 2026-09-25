using Business_Operations_Expense_Control_Platform.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDisplayInfoDto>> GetAllDepartmentsAsync();
        Task<DepartmentDisplayInfoDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentDisplayInfoDto> CreateDepartmentAsync(CreateDepartmentDto dto);
        Task<bool> DeleteDepartmentAsync(int id);

    }
}
