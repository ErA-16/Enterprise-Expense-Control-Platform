using Business_Operations_Expense_Control_Platform.Controllers;
using Business_Operations_Expense_Control_Platform.Data;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseControl.Test
{
    // Controller-level tests: the service is faked with Moq, so nothing here touches a database.
    // These only check that the controller returns the right HTTP result for what the service gives it.
    public class UserControllerTests
    {
        // DatabaseContext is still a constructor parameter on UserController (unused, left over
        // from before the refactor) so we still need to hand it *something* real to build one.
        private static DatabaseContext BuildUnusedContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        private static UserController BuildController(Mock<IUserService> mockService)
        {
            var mockLogger = new Mock<ILogger<UserController>>();
            return new UserController(mockLogger.Object, mockService.Object); // fixed — no db parameter
        }

        [Fact]
        public async Task GetAllEmployees_ServiceReturnsList_ReturnsOkWithList()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var expected = new List<UserDisplayDto> { new() { Id = 1, Fullname = "Taiwo Oni", Email = "t@test.com", DepartmentName = "Engineering" } };
            mockService.Setup(s => s.GetAllEmployeesAsync(null, null, 1, 10)).ReturnsAsync(expected);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetAllEmployees(null, null, 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetAllEmployees_ServiceThrows_Returns500()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetAllEmployeesAsync(null, null, 1, 10)).ThrowsAsync(new Exception("db exploded"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetAllEmployees(null, null, 1, 10);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetEmployeeById_ServiceReturnsUser_ReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var expected = new UserDisplayDto { Id = 1, Fullname = "Taiwo Oni", Email = "t@test.com", DepartmentName = "Engineering" };
            mockService.Setup(s => s.GetEmployeeByIdAsync(1)).ReturnsAsync(expected);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetEmployeeById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetEmployeeById_ServiceThrowsNotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetEmployeeByIdAsync(999)).ThrowsAsync(new EmployeeNotFoundException("Employee with ID 999 not found"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetEmployeeById(999);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("999", notFound.Value!.ToString());
        }

        [Fact]
        public async Task PatchEmployeeDetail_Success_ReturnsOkWithUpdatedDto()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var dto = new PatchUserInfo { Email = "new@test.com" };
            var updated = new UserDisplayDto { Id = 1, Fullname = "Taiwo Oni", Email = "new@test.com", DepartmentName = "Engineering" };
            mockService.Setup(s => s.PatchEmployeeDetailAsync(1, dto)).ReturnsAsync(updated);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.PatchEmployeeDetail(1, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updated, okResult.Value);
        }

        [Fact]
        public async Task PatchEmployeeDetail_NotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var dto = new PatchUserInfo();
            mockService.Setup(s => s.PatchEmployeeDetailAsync(999, dto)).ThrowsAsync(new EmployeeNotFoundException("Employee with ID 999 not found"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.PatchEmployeeDetail(999, dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PatchEmployeeDetail_EmailTaken_Returns400()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var dto = new PatchUserInfo { Email = "taken@test.com" };
            mockService.Setup(s => s.PatchEmployeeDetailAsync(1, dto)).ThrowsAsync(new EmailAlreadyExistsException("This email is already used by anotehr employee"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.PatchEmployeeDetail(1, dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PatchEmployeeDetail_DepartmentDoesNotExist_Returns400()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            var dto = new PatchUserInfo { DepartmentId = 999 };
            mockService.Setup(s => s.PatchEmployeeDetailAsync(1, dto)).ThrowsAsync(new DepartmentDoesNotExistException("Selected department does not exist"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.PatchEmployeeDetail(1, dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_Success_ReturnsNoContent()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.DeleteEmployeeAsync(1)).ReturnsAsync(true);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.DeleteEmployee(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_NotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.DeleteEmployeeAsync(999)).ThrowsAsync(new EmployeeNotFoundException("Employee not found"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.DeleteEmployee(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
