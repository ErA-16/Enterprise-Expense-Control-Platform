using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class CreateDepartmentDto
    {
        [Required]
        public string Name { get; set; } = null!;
    }
}
