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
    // These test the actual business logic that used to live in the controller,
    // now living in UserService. Real InMemory database, no mocking the data layer.
    public class UserServiceTests
    {
        private static DatabaseContext BuildContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        private static UserService BuildService(DatabaseContext db)
        {
            var mockLogger = new Mock<ILogger<UserService>>();
            return new UserService(db, mockLogger.Object);
        }

        private static UserDepartment SeedDepartment(DatabaseContext db, int id, string name)
        {
            var department = new UserDepartment { Id = id, Name = name };
            db.Departments.Add(department);
            db.SaveChanges();
            return department;
        }

        private static User SeedUser(DatabaseContext db, int id, string firstName, string lastName, string email, int departmentId)
        {
            var user = new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = "fake-hash",
                Role = UserRole.Employee,
                DepartmentId = departmentId
            };
            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

        // ================= GetAllEmployeesAsync — regression tests for the ignored-filter bug =================

        [Fact]
        public async Task GetAllEmployeesAsync_SearchTerm_OnlyReturnsMatchingNames()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", "taiwo@test.com", 1);
            SeedUser(db, 2, "Chidi", "Eze", "chidi@test.com", 1);
            var service = BuildService(db);

            // Act
            var result = await service.GetAllEmployeesAsync("taiwo", null, 1, 10);

            // Assert
            var user = Assert.Single(result);
            Assert.Equal("Taiwo Oni", user.Fullname);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_DepartmentFilter_OnlyReturnsThatDepartment()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedDepartment(db, 2, "Marketing");
            SeedUser(db, 1, "Taiwo", "Oni", "taiwo@test.com", 1);
            SeedUser(db, 2, "Chidi", "Eze", "chidi@test.com", 2);
            var service = BuildService(db);

            // Act
            var result = await service.GetAllEmployeesAsync(null, "marketing", 1, 10);

            // Assert
            var user = Assert.Single(result);
            Assert.Equal("Marketing", user.DepartmentName);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_Pagination_ReturnsOnlyRequestedPage()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            for (int i = 1; i <= 5; i++)
            {
                SeedUser(db, i, $"Person{i}", "Test", $"person{i}@test.com", 1);
            }
            var service = BuildService(db);

            // Act — page 1, 2 per page
            var page1 = await service.GetAllEmployeesAsync(null, null, 1, 2);

            // Assert
            Assert.Equal(2, page1.Count);
        }

        // ================= GetEmployeeByIdAsync =================

        [Fact]
        public async Task GetEmployeeByIdAsync_DoesNotExist_ThrowsEmployeeNotFoundException()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeNotFoundException>(() => service.GetEmployeeByIdAsync(999));
        }

        // ================= PatchEmployeeDetailAsync =================

        [Fact]
        public async Task PatchEmployeeDetailAsync_OnlyEmailProvided_LeavesOtherFieldsUnchanged()
        {
            // Arrange — the original null-wipe regression test, still relevant after the refactor
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", "old@test.com", 1);
            var service = BuildService(db);
            var dto = new PatchUserInfo { Email = "new@test.com" };

            // Act
            var result = await service.PatchEmployeeDetailAsync(1, dto);

            // Assert
            Assert.Equal("new@test.com", result.Email);
            Assert.Equal("Taiwo Oni", result.Fullname); // unchanged
        }

        [Fact]
        public async Task PatchEmployeeDetailAsync_EmailTakenByAnotherUser_ThrowsEmailAlreadyExistsException()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", "taiwo@test.com", 1);
            SeedUser(db, 2, "Chidi", "Eze", "chidi@test.com", 1);
            var service = BuildService(db);
            var dto = new PatchUserInfo { Email = "chidi@test.com" };

            // Act & Assert
            await Assert.ThrowsAsync<EmailAlreadyExistsException>(() => service.PatchEmployeeDetailAsync(1, dto));
        }

        [Fact]
        public async Task PatchEmployeeDetailAsync_DepartmentDoesNotExist_ThrowsDepartmentDoesNotExistException()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", "taiwo@test.com", 1);
            var service = BuildService(db);
            var dto = new PatchUserInfo { DepartmentId = 999 };

            // Act & Assert
            await Assert.ThrowsAsync<DepartmentDoesNotExistException>(() => service.PatchEmployeeDetailAsync(1, dto));
        }

        // ================= DeleteEmployeeAsync =================

        [Fact]
        public async Task DeleteEmployeeAsync_Exists_RemovesUser()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", "taiwo@test.com", 1);
            var service = BuildService(db);

            // Act
            var result = await service.DeleteEmployeeAsync(1);

            // Assert
            Assert.True(result);
            Assert.Empty(db.Users);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_DoesNotExist_ThrowsEmployeeNotFoundException()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);

            // Act & Assert
            await Assert.ThrowsAsync<EmployeeNotFoundException>(() => service.DeleteEmployeeAsync(999));
        }
    }
}
