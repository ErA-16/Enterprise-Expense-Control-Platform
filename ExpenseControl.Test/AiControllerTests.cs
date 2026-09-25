using Business_Operations_Expense_Control_Platform.Controllers;
using Business_Operations_Expense_Control_Platform.Dtos;
using Business_Operations_Expense_Control_Platform.Services;
using Business_Operations_Expense_Control_Platform.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseControl.Test
{
    public class AiControllerTests
    {
        private static AiController BuildController(Mock<IAiService> mockService)
        {
            return new AiController(mockService.Object);
        }

        [Fact]
        public async Task GetExpenseAssist_EmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IAiService>();
            var controller = BuildController(mockService);
            var request = new AiAssistRequestDto { Title = "", Amount = 5000m };

            // Act
            var result = await controller.GetExpenseAssist(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetExpenseAssist_Success_ReturnsOk()
        {
            // Arrange
            var mockService = new Mock<IAiService>();
            var request = new AiAssistRequestDto { Title = "27\" Monitor", Reason = "Old one broke", Amount = 185000m };
            var expected = new AiAssistResponseDto { SuggestedTitle = "27-inch Monitor Replacement", SuggestedReason = "Current monitor is damaged and needs replacing." };
            mockService.Setup(s => s.GetExpenseAssistAsync(request)).ReturnsAsync(expected);
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetExpenseAssist(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetExpenseAssist_AiServiceException_Returns502()
        {
            // Arrange
            var mockService = new Mock<IAiService>();
            var request = new AiAssistRequestDto { Title = "Monitor", Amount = 5000m };
            mockService.Setup(s => s.GetExpenseAssistAsync(request))
                .ThrowsAsync(new AiServiceException("AI response could not be parsed into the expected format."));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetExpenseAssist(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(502, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetExpenseAssist_UnexpectedException_Returns500()
        {
            // Arrange
            var mockService = new Mock<IAiService>();
            var request = new AiAssistRequestDto { Title = "Monitor", Amount = 5000m };
            mockService.Setup(s => s.GetExpenseAssistAsync(request)).ThrowsAsync(new Exception("network down"));
            var controller = BuildController(mockService);

            // Act
            var result = await controller.GetExpenseAssist(request);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }
    }
}
