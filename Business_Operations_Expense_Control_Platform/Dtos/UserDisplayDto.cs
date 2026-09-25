namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class UserDisplayDto
    {
        public int Id { get; set; }
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
    }
}
