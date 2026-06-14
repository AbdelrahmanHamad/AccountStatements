using Microsoft.EntityFrameworkCore;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;
using AccountStatements.Infrastructure.Data;

namespace AccountStatements.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.Transactions.AddAsync(transaction, cancellationToken);
        }

        public async Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime date, CancellationToken cancellationToken = default)
        {
            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customerId && t.TransactionDate < date)
                .Select(t => t.Amount)
                .ToListAsync(cancellationToken);

            return transactions.Sum();
        }

        public async Task<List<Transaction>> GetTransactionsByCustomerAndPeriodAsync(Guid customerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.Transactions
                .Where(t => t.CustomerId == customerId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderBy(t => t.TransactionDate)
                .ToListAsync(cancellationToken);
        }
    }
}
