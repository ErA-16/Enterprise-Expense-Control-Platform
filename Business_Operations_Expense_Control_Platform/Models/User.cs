namespace Business_Operations_Expense_Control_Platform.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }
        public int DepartmentId { get; set; }
        public UserDepartment Department { get; set; } = null!;
        public List<ExpenseRequest> ExpenseRequests { get; set; } = null!;
    }
}
