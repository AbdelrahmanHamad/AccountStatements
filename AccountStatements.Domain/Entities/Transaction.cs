using System.ComponentModel.DataAnnotations;
using AccountStatements.Domain.Common;

namespace AccountStatements.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public decimal Amount { get; set; } 

        [Required]
        [MaxLength(250)]
        public string Description { get; set; } = null!;

        [Required]
        public DateTime TransactionDate { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
