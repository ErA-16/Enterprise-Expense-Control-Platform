namespace Business_Operations_Expense_Control_Platform.Models
{
    public class RequestAudit
    {
        public int Id { get; set; }
        public int RequestId { get; set; }             
        public ExpenseRequest Request { get; set; } = null!;  
        public RequestAuditAction Action { get; set; }        
        public int ActorId { get; set; }                 
        public User Actor { get; set; } = null!;         
        public DateTime Timestamp { get; set; }          
    }
}
