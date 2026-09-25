namespace Business_Operations_Expense_Control_Platform.Services.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }
    public class FileUploadException : Exception
    {
        public FileUploadException(string message) : base(message) { }
    }
    public class ManagerWithIdNotFoundException : Exception
    {
        public ManagerWithIdNotFoundException(string message) : base(message) { }
    }
    public class ExpenseNotFoundException : Exception
    {
        public ExpenseNotFoundException(string message) : base(message) { }
    }
    public class ExpenseAlreadyProcessingException : Exception
    {
        public ExpenseAlreadyProcessingException(string message) : base(message) { }
    }
    public class UnauthorizedExpenseReviewException : Exception
    {
        public UnauthorizedExpenseReviewException(string message) : base(message) { }
    }
    public class UploadProofOfPaymentException : Exception
    {
        public UploadProofOfPaymentException(string message) : base(message) { }
    }
}