using Business_Operations_Expense_Control_Platform.Controllers;
using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Hubs;
using Business_Operations_Expense_Control_Platform.Models;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace ExpenseControl.Test
{
    public class ExpenseControllerTests
    {
        private static DatabaseContext BuildUnusedContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        // FIXED: Converted from a 'var' statement into a proper private class field
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext = new Mock<IHubContext<NotificationHub>>();

        private static ExpenseController BuildController(Mock<IExpenseService> mockService, ClaimsPrincipal? user = null)
        {
            var mockLogger = new Mock<ILogger<ExpenseController>>();
            var controller = new ExpenseController(mockLogger.Object, mockService.Object); // fixed — no db parameter

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal() }
            };

            return controller;
        }

        private static ClaimsPrincipal BuildClaimsUser(int id, string role, int departmentId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("DepartmentId", departmentId.ToString())
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        // ================= CreateExpense =================

        [Fact]
        public async Task CreateExpense_ValidUser_ReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new CreateExpenseDto { Title = "Monitor", Amount = 185000m };
            var expected = new ExpenseRequestDisplayInfoDto { Id = 1, Title = "Monitor", Amount = 185000m, Status = ExpenseRequestStatus.Pending };
            mockService.Setup(s => s.CreateExpenseAsync(dto, 1)).ReturnsAsync(expected);
            var controller = BuildController(mockService, BuildClaimsUser(1, "Employee", 1));

            // Act
            var result = await controller.CreateExpense(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateExpense_MissingUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var controller = BuildController(mockService); // no claims
            var dto = new CreateExpenseDto { Title = "Chair", Amount = 5000m };

            // Act
            var result = await controller.CreateExpense(dto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task CreateExpense_ServiceThrowsUserNotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new CreateExpenseDto { Title = "Chair", Amount = 5000m };
            mockService.Setup(s => s.CreateExpenseAsync(dto, 1)).ThrowsAsync(new UserNotFoundException("Profile not found"));
            var controller = BuildController(mockService, BuildClaimsUser(1, "Employee", 1));

            // Act
            var result = await controller.CreateExpense(dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateExpense_ServiceThrowsFileUploadException_Returns400()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new CreateExpenseDto { Title = "Chair", Amount = 5000m };
            mockService.Setup(s => s.CreateExpenseAsync(dto, 1)).ThrowsAsync(new FileUploadException("Only JPG, PNG, or PDF files are allowed"));
            var controller = BuildController(mockService, BuildClaimsUser(1, "Employee", 1));

            // Act
            var result = await controller.CreateExpense(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ================= GetMyOwnRequests =================

        [Fact]
        public async Task GetMyOwnRequests_MissingClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetMyOwnRequests();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task GetMyOwnRequests_Success_ReturnsOkWithList()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var expected = new List<ExpenseRequestDisplayInfoDto> { new() { Id = 1, Title = "Monitor" } };
            mockService.Setup(s => s.GetMyOwnRequestsAsync(1)).ReturnsAsync(expected);
            var controller = BuildController(mockService, BuildClaimsUser(1, "Employee", 1));

            // Act
            var result = await controller.GetMyOwnRequests();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        // ================= GetDepartmentRequests =================

        [Fact]
        public async Task GetDepartmentRequests_MissingDepartmentClaim_ReturnsUnauthorized()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "10") };
            var controller = BuildController(mockService, new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth")));

            // Act
            var result = await controller.GetDepartmentRequests();

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        // ================= ProcessRequest (manager) =================

        [Fact]
        public async Task ManagerProcessRequest_Approve_ReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new ManagerReviewExpenseDto { IsApproved = true };
            mockService.Setup(s => s.ProcessRequestAsync(1, dto, 10, 1)).ReturnsAsync(true);
            var controller = BuildController(mockService, BuildClaimsUser(10, "Manager", 1));

            // Act
            var result = await controller.ProcessRequest(1, dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ManagerProcessRequest_SelfApproval_ReturnsForbid()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new ManagerReviewExpenseDto { IsApproved = true };
            mockService.Setup(s => s.ProcessRequestAsync(1, dto, 10, 1))
                .ThrowsAsync(new UnauthorizedExpenseReviewException("Self-approval is strictly prohibited."));
            var controller = BuildController(mockService, BuildClaimsUser(10, "Manager", 1));

            // Act
            var result = await controller.ProcessRequest(1, dto);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ManagerProcessRequest_AlreadyProcessed_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IExpenseService>();
            var dto = new ManagerReviewExpenseDto { IsApproved = true };
            mockService.Setup(s => s.ProcessRequestAsync(1, dto, 10, 1))
                .ThrowsAsync(new InvalidOperationException("Request has already been processed."));
            var controller = BuildController(mockService, BuildClaimsUser(10, "Manager", 1));

            // Act
            var result = await controller.ProcessRequest(1, dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
