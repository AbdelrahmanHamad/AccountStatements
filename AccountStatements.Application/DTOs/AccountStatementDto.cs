
namespace AccountStatements.Application.DTOs
{
    public class AccountStatementDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CustomerEmail { get; set; } = null!;
        public string StatementMonth { get; set; } = null!;
        public decimal StartingBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string EmailSentStatus { get; set; } = null!;
        public DateTime? SentAt { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
    }
}
