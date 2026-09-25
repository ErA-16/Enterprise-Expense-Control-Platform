using Business_Operations_Expense_Control_Platform.Models;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class ExpenseRequestDisplayInfoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string DateAndTime { get; set; } = null!;
        public ExpenseRequestStatus Status { get; set; }
        public int? ApprovedById { get; set; }
        public string? ApprovedAt { get; set; }
        public int? RejectedById { get; set; }
        public string? RejectedAt { get; set; }
        public string? RejectedByRole { get; set; }
        public string? ManagerComment { get; set; }
        public int? PaidById { get; set; }
        public string? PaidAt { get; set; }
        public Models.PaymentMethod? PaymentMethod { get; set; }
    }
}
