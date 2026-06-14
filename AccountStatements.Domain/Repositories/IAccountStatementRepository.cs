using AccountStatements.Domain.Entities;

namespace AccountStatements.Domain.Repositories
{
    public interface IAccountStatementRepository
    {
        Task AddAsync(AccountStatement statement, CancellationToken cancellationToken = default);
        Task<List<AccountStatement>> GetStatementsAsync(Guid? customerId, string? month, CancellationToken cancellationToken = default);
        Task<AccountStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid customerId, string month, CancellationToken cancellationToken = default);
    }
}
