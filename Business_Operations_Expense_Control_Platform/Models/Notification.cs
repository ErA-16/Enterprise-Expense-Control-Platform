namespace Business_Operations_Expense_Control_Platform.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int ReceiverId { get; set; }
        public User Receiver { get; set; } = null!;
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int? RequestId { get; set; }
        public ExpenseRequest? Request { get; set; }
    }
}
