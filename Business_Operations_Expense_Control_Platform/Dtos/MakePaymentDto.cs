using Business_Operations_Expense_Control_Platform.Models;
using System.ComponentModel.DataAnnotations;

namespace Business_Operations_Expense_Control_Platform.Dtos
{
    public class MakePaymentDto
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public IFormFile PaymentProof { get; set; } = null!;
    }
}
