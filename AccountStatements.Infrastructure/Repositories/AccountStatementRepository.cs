using Microsoft.EntityFrameworkCore;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;
using AccountStatements.Infrastructure.Data;

namespace AccountStatements.Infrastructure.Repositories
{
    public class AccountStatementRepository : IAccountStatementRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountStatementRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccountStatement statement, CancellationToken cancellationToken = default)
        {
            await _context.AccountStatements.AddAsync(statement, cancellationToken);
        }

        public async Task<List<AccountStatement>> GetStatementsAsync(Guid? customerId, string? month, CancellationToken cancellationToken = default)
        {
            var query = _context.AccountStatements
                .Include(s => s.Customer)
                .AsQueryable();

            if (customerId.HasValue && customerId.Value != Guid.Empty)
            {
                query = query.Where(s => s.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(month))
            {
                query = query.Where(s => s.StatementMonth == month);
            }

            return await query.OrderByDescending(s => s.GeneratedAt).ToListAsync(cancellationToken);
        }

        public async Task<AccountStatement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AccountStatements
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid customerId, string month, CancellationToken cancellationToken = default)
        {
            return await _context.AccountStatements
                .AnyAsync(s => s.CustomerId == customerId && s.StatementMonth == month, cancellationToken);
        }
    }
}
