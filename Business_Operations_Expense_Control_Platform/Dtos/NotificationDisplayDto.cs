namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class NotificationDisplayDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public string CreatedAt { get; set; } = null!;
        public int? RequestId { get; set; }
    }
}
