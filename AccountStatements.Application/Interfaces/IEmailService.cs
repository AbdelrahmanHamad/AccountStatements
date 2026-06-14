using AccountStatements.Application.DTOs;

namespace AccountStatements.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendStatementEmailAsync(string recipientEmail, string recipientName, AccountStatementDto statement, CancellationToken cancellationToken = default);
    }
}
