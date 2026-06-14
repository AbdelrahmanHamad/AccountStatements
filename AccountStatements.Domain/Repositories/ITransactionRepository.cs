using AccountStatements.Domain.Entities;

namespace AccountStatements.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
        Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime date, CancellationToken cancellationToken = default);
        Task<List<Transaction>> GetTransactionsByCustomerAndPeriodAsync(Guid customerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
