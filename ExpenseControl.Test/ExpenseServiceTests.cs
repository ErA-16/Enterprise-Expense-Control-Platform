using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Hubs; // Added for NotificationHub
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR; // Added for IHubContext
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseControl.Test
{
    public class ExpenseServiceTests
    {
        private static DatabaseContext BuildContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        // FIXED: Added the required third parameter 'hubContext' using Moq
        private static ExpenseService BuildService(DatabaseContext db)
        {
            var mockLogger = new Mock<ILogger<ExpenseService>>();
            var mockHubContext = new Mock<IHubContext<NotificationHub>>();

            return new ExpenseService(db, mockLogger.Object, mockHubContext.Object);
        }

        private static UserDepartment SeedDepartment(DatabaseContext db, int id, string name)
        {
            var department = new UserDepartment { Id = id, Name = name };
            db.Departments.Add(department);
            db.SaveChanges();
            return department;
        }

        private static User SeedUser(DatabaseContext db, int id, string firstName, string lastName, UserRole role, int departmentId)
        {
            var user = new User
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = $"{firstName.ToLower()}@test.com",
                PasswordHash = "fake-hash",
                Role = role,
                DepartmentId = departmentId
            };
            db.Users.Add(user);
            db.SaveChanges();
            return user;
        }

        private static ExpenseRequest SeedExpenseRequest(
            DatabaseContext db, int id, int requesterId, int departmentId,
            ExpenseRequestStatus status, decimal amount = 10000m)
        {
            var request = new ExpenseRequest
            {
                Id = id,
                RequesterId = requesterId,
                DepartmentId = departmentId,
                Title = "27\" Monitor",
                Reason = "Current monitor is damaged",
                Amount = amount,
                DateAndTime = DateTime.UtcNow,
                Status = status
            };
            db.ExpenseRequests.Add(request);
            db.SaveChanges();
            return request;
        }

        // Real IFormFile, backed by an in-memory byte array — file endpoints need an actual file to copy.
        // Note: SaveUploadedFileAsync writes to a real "uploads" folder on disk when these run.
        private static IFormFile BuildFakeFile(string contentType = "application/pdf")
        {
            var content = "fake file content"u8.ToArray();
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "file", "test.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        // ================= CreateExpenseAsync =================

        [Fact]
        public async Task CreateExpenseAsync_ValidEmployee_CreatesRequestAuditAndNotifiesManagers()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", UserRole.Employee, 1);
            SeedUser(db, 10, "Manager", "Person", UserRole.Manager, 1);
            var service = BuildService(db);
            var dto = new CreateExpenseDto { Title = "27\" Monitor", Reason = "Damaged", Amount = 185000m };

            // Act
            var result = await service.CreateExpenseAsync(dto, 1);

            // Assert
            Assert.Equal(ExpenseRequestStatus.Pending, result.Status);
            Assert.Single(db.ExpenseRequests);
            Assert.Single(db.RequestAudits);
            var notification = Assert.Single(db.Notifications);
            Assert.Equal(10, notification.ReceiverId);
        }

        [Fact]
        public async Task CreateExpenseAsync_UserNotFound_ThrowsUserNotFoundException()
        {
            // Arrange
            var db = BuildContext();
            var service = BuildService(db);
            var dto = new CreateExpenseDto { Title = "Chair", Amount = 5000m };

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => service.CreateExpenseAsync(dto, 999));
        }

        // ================= ProcessRequestAsync (manager) =================

        [Fact]
        public async Task ProcessRequestAsync_ManagerApproves_SetsApprovedAndAddsAudit()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", UserRole.Employee, 1);
            SeedUser(db, 10, "Manager", "Person", UserRole.Manager, 1);
            var request = SeedExpenseRequest(db, 1, requesterId: 1, departmentId: 1, ExpenseRequestStatus.Pending);
            var service = BuildService(db);
            var dto = new ManagerReviewExpenseDto { IsApproved = true };

            // Act
            var result = await service.ProcessRequestAsync(request.Id, dto, currentManagerId: 10, currentManagerDeptId: 1);

            // Assert
            Assert.True(result);
            var updated = db.ExpenseRequests.Single(r => r.Id == request.Id);
            Assert.Equal(ExpenseRequestStatus.Approved, updated.Status);
            Assert.Equal(10, updated.ApprovedById);
        }

        [Fact]
        public async Task ProcessRequestAsync_ManagerIsRequester_ThrowsUnauthorizedExpenseReviewException()
        {
            // Arrange — a manager cannot approve/reject their own request
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 10, "Manager", "Person", UserRole.Manager, 1);
            var request = SeedExpenseRequest(db, 1, requesterId: 10, departmentId: 1, ExpenseRequestStatus.Pending);
            var service = BuildService(db);
            var dto = new ManagerReviewExpenseDto { IsApproved = true };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedExpenseReviewException>(
                () => service.ProcessRequestAsync(request.Id, dto, currentManagerId: 10, currentManagerDeptId: 1));
        }

        [Fact]
        public async Task ProcessRequestAsync_DifferentDepartment_ThrowsUnauthorizedExpenseReviewException()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedDepartment(db, 2, "Marketing");
            SeedUser(db, 1, "Taiwo", "Oni", UserRole.Employee, 2);
            SeedUser(db, 10, "Manager", "Person", UserRole.Manager, 1);
            var request = SeedExpenseRequest(db, 1, requesterId: 1, departmentId: 2, ExpenseRequestStatus.Pending);
            var service = BuildService(db);
            var dto = new ManagerReviewExpenseDto { IsApproved = true };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedExpenseReviewException>(
                () => service.ProcessRequestAsync(request.Id, dto, currentManagerId: 10, currentManagerDeptId: 1));
        }

        [Fact]
        public async Task ProcessRequestAsync_RequestAlreadyProcessed_ThrowsExpenseAlreadyProcessingException()
        {
            // Arrange
            var db = BuildContext();
            SeedDepartment(db, 1, "Engineering");
            SeedUser(db, 1, "Taiwo", "Oni", UserRole.Employee, 1);
            SeedUser(db, 10, "Manager", "Person", UserRole.Manager, 1);
            var request = SeedExpenseRequest(db, 1, requesterId: 1, departmentId: 1, ExpenseRequestStatus.Approved);
            var service = BuildService(db);
            var dto = new ManagerReviewExpenseDto { IsApproved = true };

            // Act & Assert
            // FIXED: Fully completed the truncated code snippet
            await Assert.ThrowsAsync<ExpenseAlreadyProcessingException>(
                () => service.ProcessRequestAsync(request.Id, dto, currentManagerId: 10, currentManagerDeptId: 1));
        }
    }
}
