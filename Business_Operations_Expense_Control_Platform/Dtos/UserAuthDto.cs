using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class UserAuthDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
