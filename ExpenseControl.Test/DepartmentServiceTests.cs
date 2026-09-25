using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseControl.Test
{
    public class DepartmentServiceTests
    {
        private static DatabaseContext BuildContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        private static DepartmentService BuildService(DatabaseContext db)
        {
            var mockLogger = new Mock<ILogger<DepartmentService>>();
            return new DepartmentService(db, mockLogger.Object);
        }

        private static UserDepartment SeedDepartment(DatabaseContext db, int id, string name)
        {
            var department = new UserDepartment { Id = id, Name = name };
            db.Departments.Add(department);
            db.SaveChanges();
            return department;
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ReturnsEveryDepartment()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedDepartment(db, 2, "Marketing");
            var service = BuildService(db);

            // Act
            var result = await service.GetAllDepartmentsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_DoesNotExist_ThrowsDepartmentNotFoundException()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);

            // Act & Assert
            await Assert.ThrowsAsync<DepartmentNotFoundException>(() => service.GetDepartmentByIdAsync(999));
        }

        [Fact]
        public async Task CreateDepartmentAsync_NewName_AddsDepartment()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);
            var dto = new CreateDepartmentDto { Name = "Finance" };

            // Act
            var result = await service.CreateDepartmentAsync(dto);

            // Assert
            Assert.Equal("Finance", result.Name);
            Assert.Single(db.Departments);
        }

        [Fact]
        public async Task CreateDepartmentAsync_NameAlreadyExists_ThrowsDepartmentAlreadyExistException()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            var service = BuildService(db);
            var dto = new CreateDepartmentDto { Name = "Engineering" };

            // Act & Assert
            await Assert.ThrowsAsync<DepartmentAlreadyExistException>(() => service.CreateDepartmentAsync(dto));
        }

        [Fact]
        public async Task DeleteDepartmentAsync_Exists_RemovesDepartment()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            var service = BuildService(db);

            // Act
            var result = await service.DeleteDepartmentAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(db.Departments);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_DoesNotExist_ThrowsDepartmentNotFoundException()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);

            // Act & Assert
            await Assert.ThrowsAsync<DepartmentNotFoundException>(() => service.DeleteDepartmentAsync(999));
        }
    }
}
