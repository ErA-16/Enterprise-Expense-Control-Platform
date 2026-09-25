using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Business_Operations_Expense_Control_Platform.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<DepartmentService> _logger;
        public DepartmentService(DatabaseContext db, ILogger<DepartmentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<DepartmentDisplayInfoDto>> GetAllDepartmentsAsync()
        {
            _logger.LogInformation("Getting all Departments");

            return await _db.Departments.Select(d => new DepartmentDisplayInfoDto
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();
        }

        public async Task<DepartmentDisplayInfoDto> GetDepartmentByIdAsync(int id)
        {
            _logger.LogInformation("Fetching Department with Id: {deptId}", id);

            var selectedDept = await _db.Departments.Where(d => d.Id == id).Select(u => new DepartmentDisplayInfoDto
            {
                Id = u.Id,
                Name = u.Name
            }).FirstOrDefaultAsync();

            if (selectedDept == null)
            {
                _logger.LogWarning("Lookup failed no Department with Id: {deptId}", id);
                throw new DepartmentNotFoundException("Department not found");
            }

            return selectedDept;
        }

        public async Task<DepartmentDisplayInfoDto> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            _logger.LogInformation("Creating department for {dept}", dto.Name);
            var departmentExists = await _db.Departments.AnyAsync(d => d.Name == dto.Name);

            if (departmentExists)
            {
                _logger.LogWarning("Department registration failed. Department {dept} already exists.", dto.Name);
                throw new DepartmentAlreadyExistException("Department already exists");
            }

            var newDepartment = new UserDepartment
            {
                Name = dto.Name
            };

            _db.Departments.Add(newDepartment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Department '{deptName}' created sucessfully", newDepartment.Name);

            return new DepartmentDisplayInfoDto
            {
                Id = newDepartment.Id,
                Name = dto.Name
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            _logger.LogInformation("Deleting Department Id: {deptId}", id);
            var selectedDepartment = await _db.Departments.FindAsync(id);

            if (selectedDepartment == null)
            {
                _logger.LogWarning("Department with Id not found: {deptId}", id);
                throw new DepartmentNotFoundException("Department not found");
            }

            _logger.LogInformation("Department {deptId} successfully deleted", id);

            _db.Departments.Remove(selectedDepartment);
            await _db.SaveChangesAsync();

            return true;
        }

    }
}
