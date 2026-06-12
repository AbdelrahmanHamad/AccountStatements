using System.ComponentModel.DataAnnotations;
using AccountStatements.Domain.Common;

namespace AccountStatements.Domain.Entities
{
    public class AccountStatement : BaseEntity
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [MaxLength(7)] 
        public string StatementMonth { get; set; } = null!;

        [Required]
        public decimal StartingBalance { get; set; }

        [Required]
        public decimal EndingBalance { get; set; }

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string EmailSentStatus { get; set; } = "Pending";

        public DateTime? SentAt { get; set; }

        public Customer Customer { get; set; } = null!;
    }
}
