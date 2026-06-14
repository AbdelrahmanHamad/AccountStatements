using AccountStatements.Application.DTOs;

namespace AccountStatements.Application.Interfaces
{
    public interface IEmailJob
    {
        Task SendEmailAndUpdateStatusAsync(Guid statementId, AccountStatementDto statementDto, CancellationToken cancellationToken);
    }
}
