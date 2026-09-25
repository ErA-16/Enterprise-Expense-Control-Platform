namespace Business_Operations_Expense_Control_Platform.Services.Exceptions
{
    public class EmployeeNotFoundException : Exception
    {
        public EmployeeNotFoundException(string message) : base(message) { }
    }
    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string message) : base(message) { }
    }
    public class DepartmentDoesNotExistException : Exception
    {
        public DepartmentDoesNotExistException(string message) : base(message) { }
    }
}
