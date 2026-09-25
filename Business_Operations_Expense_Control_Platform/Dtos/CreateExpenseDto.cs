using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class CreateExpenseDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(100, ErrorMessage = "Title shold not exceed 100 characters.")]
        public string Title { get; set; } = null!;
        [MaxLength(500, ErrorMessage = "Reason should be less than 500 characters.")]
        public string? Reason { get; set; }
        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be at least 1.")]
        public decimal Amount { get; set; }
        public IFormFile? Receipt { get; set; }
    }
}
