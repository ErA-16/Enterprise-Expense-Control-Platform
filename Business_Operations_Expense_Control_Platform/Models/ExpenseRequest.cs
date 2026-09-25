using System.ComponentModel.DataAnnotations.Schema;

namespace Business_Operations_Expense_Control_Platform.Models
{
    public class ExpenseRequest
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public User Requester { get; set; } = null!;
        public string Title { get; set; } = null!;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public int DepartmentId { get; set; }
        public UserDepartment Department { get; set; } = null!;
        public string? Reason { get; set; }
        public DateTime DateAndTime { get; set; }
        public ExpenseRequestStatus Status { get; set; }
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedById { get; set; }
        public DateTime? RejectedAt { get; set; }
        public RejectionRole? RejectedByRole { get; set; }
        public string? ManagerComment { get; set; }
        public int? PaidById { get; set; }
        public DateTime? PaidAt { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? SubmissionReceiptPath { get; set; }
        public string? PaymentProofPath { get; set; }
        public DateTime? ReminderSentAt { get; set; }
    }
}