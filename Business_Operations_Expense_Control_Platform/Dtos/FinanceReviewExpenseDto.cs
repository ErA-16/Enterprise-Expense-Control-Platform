using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class FinanceReviewExpenseDto
    {
        [Required]
        public bool IsApproved { get; set;  }
    }
}