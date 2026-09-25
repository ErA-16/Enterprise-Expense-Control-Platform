using Business_Operations_Expense_Control_Platform.Models;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class RequestAuditDisplayDto
    {
        public RequestAuditAction Action { get; set; }
        public int ActorId { get; set; }
        public string ActorName { get; set; } = null!;
        public string Timestamp { get; set; } = null!;
    }
}
