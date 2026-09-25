namespace Business_Operations_Expense_Control_Platform.Services.Exceptions
{
    public class DepartmentNotFoundException : Exception
    {
        public DepartmentNotFoundException(string message) : base(message) { }
    }
    public class DepartmentAlreadyExistException : Exception
    {
        public DepartmentAlreadyExistException(string message) : base(message) { }
    }
}
