using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class ManagerReviewExpenseDto
    {
        [Required]
        public bool IsApproved { get; set; }
        [MaxLength(250, ErrorMessage = "Comments cannot exceed 250 characters.")]
        public string? ManagerComment { get; set; }
    }
}
