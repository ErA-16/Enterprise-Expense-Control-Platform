using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<UserService> _logger;

        public UserService(DatabaseContext db, ILogger<UserService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<UserDisplayDto>> GetAllEmployeesAsync(string? searchTerm, string? departmentFilter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Processing employee list request.");

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(u => u.FirstName.ToLower().Contains(searchTerm) ||
                                        u.LastName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(departmentFilter))
            {
                departmentFilter = departmentFilter.Trim().ToLower();
                query = query.Where(d => d.Department.Name.ToLower() == departmentFilter);
            }

            query = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);

            var recordsToSkip = (pageNumber - 1) * pageSize;

            query = query.Skip(recordsToSkip).Take(pageSize);

            return await query
            .Select(u => new UserDisplayDto { Id = u.Id, Fullname = u.FirstName + " " + u.LastName, Email = u.Email, DepartmentName = u.Department.Name })
            .ToListAsync();
        }

        public async Task<UserDisplayDto> GetEmployeeByIdAsync(int id)
        {
            var selectedUser = await _db.Users.Where(u => u.Id == id).Select(u => new UserDisplayDto
            {
                Id = u.Id,
                Fullname = u.FirstName + " " + u.LastName,
                Email = u.Email,
                DepartmentName = u.Department.Name
            }).FirstOrDefaultAsync();

            if (selectedUser == null)
            {
                _logger.LogWarning("Lookup failed no employee with Id: {UserId}", id);
                throw new EmployeeNotFoundException($"Employee with ID {id} not found");
            }

            _logger.LogInformation("Successfully retrieved employee details for ID: {UserId}", id);
            return selectedUser;
        }

        public async Task<UserDisplayDto> PatchEmployeeDetailAsync(int id, PatchUserInfo dto)
        {
            _logger.LogInformation("Fetching employee with Id {employeeId}", id);

            var selectedEmployee = await _db.Users.FindAsync(id);

            if (selectedEmployee == null)
            {
                _logger.LogWarning("Employee with Id: {employeeId} not found", id);
                throw new EmployeeNotFoundException($"Employee with ID {id} not found");
            }

            var emailConflict = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailConflict)
            {
                _logger.LogWarning("Update failed. Email {Email} is already taken by another user.", dto.Email);
                throw new EmailAlreadyExistsException("This email is already used by anotehr employee");
            }

            if (dto.DepartmentId != null)
            {
                var departmentExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
                if (!departmentExists) throw new DepartmentDoesNotExistException("Selected department does not exist");
            }

            if (dto.FirstName != null) selectedEmployee.FirstName = dto.FirstName;
            if (dto.LastName != null) selectedEmployee.LastName = dto.LastName;
            if (dto.Email != null) selectedEmployee.Email = dto.Email;
            if (dto.DepartmentId != null) selectedEmployee.DepartmentId = dto.DepartmentId.Value;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully updated information for employee ID: {UserId}", id);

            string deptName = "";
            if (selectedEmployee.DepartmentId != 0)
            {
                deptName = await _db.Departments
                    .Where(d => d.Id == selectedEmployee.DepartmentId)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync() ?? "";
            }

            return new UserDisplayDto
            {
                Id = selectedEmployee.Id,
                Fullname = selectedEmployee.FirstName + " " + selectedEmployee.LastName,
                Email = selectedEmployee.Email,
                DepartmentName = deptName
            };
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var selectedEmployee = await _db.Users.FindAsync(id);

            if (selectedEmployee == null)
            {
                _logger.LogWarning("Employee with Id {employeeId} not found", id);
                throw new EmployeeNotFoundException("Employee not found");
            }

            _db.Users.Remove(selectedEmployee);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Employee with Id {employeeId} successfully deleted", id);
            return true;
        }
    }
}
