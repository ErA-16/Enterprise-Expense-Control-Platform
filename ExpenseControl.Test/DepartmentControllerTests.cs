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
    public class DepartmentControllerTests
    {
        private static DatabaseContext BuildUnusedContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new DatabaseContext(options);
        }

        private static DepartmentController BuildController(Mock<IDepartmentService> mockService)
        {
            var mockLogger = new Mock<ILogger<DepartmentController>>();
            return new DepartmentController(mockLogger.Object, mockService.Object);
        }

        [Fact]
        public async Task GetAllDepartments_ReturnsOkWithList()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            var expected = new List<DepartmentDisplayInfoDto> { new() { Id = 1, Name = "Engineering" } };
            mockService.Setup(s => s.GetAllDepartmentsAsync()).ReturnsAsync(expected);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetAllDepartments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetAllDepartments_ServiceThrows_Returns500()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            mockService.Setup(s => s.GetAllDepartmentsAsync()).ThrowsAsync(new Exception("boom"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetAllDepartments();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetDepartmentById_Found_ReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            var expected = new DepartmentDisplayInfoDto { Id = 1, Name = "Engineering" };
            mockService.Setup(s => s.GetDepartmentByIdAsync(1)).ReturnsAsync(expected);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetDepartmentById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetDepartmentById_NotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            mockService.Setup(s => s.GetDepartmentByIdAsync(999)).ThrowsAsync(new DepartmentNotFoundException("Department not found"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetDepartmentById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateDepartment_Success_ReturnsCreatedAt()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            var dto = new CreateDepartmentDto { Name = "Finance" };
            var created = new DepartmentDisplayInfoDto { Id = 5, Name = "Finance" };
            mockService.Setup(s => s.CreateDepartmentAsync(dto)).ReturnsAsync(created);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.CreateDepartment(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
        }

        [Fact]
        public async Task CreateDepartment_AlreadyExists_Returns400()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            var dto = new CreateDepartmentDto { Name = "Engineering" };
            mockService.Setup(s => s.CreateDepartmentAsync(dto)).ThrowsAsync(new DepartmentAlreadyExistException("Department already exists"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.CreateDepartment(dto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteDepartment_Success_ReturnsNoContent()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            mockService.Setup(s => s.DeleteDepartmentAsync(1)).ReturnsAsync(true);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.DeleteDepartment(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteDepartment_NotFound_Returns404()
        {
            // Arrange
            var mockService = new Mock<IDepartmentService>();
            mockService.Setup(s => s.DeleteDepartmentAsync(999)).ThrowsAsync(new DepartmentNotFoundException("Department not found"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.DeleteDepartment(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
