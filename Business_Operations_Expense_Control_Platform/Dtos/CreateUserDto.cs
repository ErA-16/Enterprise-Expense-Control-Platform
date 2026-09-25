using Business_Operations_Expense_Control_Platform.Models;
using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class CreateUserDto
    {
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public int DepartmentId { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;
    }
}
