using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AccountStatements.Application.DTOs;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Statements.Queries.GetStatementById
{
    public record GetStatementByIdQuery(Guid Id) : IRequest<AccountStatementDto?>;

    public class GetStatementByIdQueryHandler : IRequestHandler<GetStatementByIdQuery, AccountStatementDto?>
    {
        private readonly IAccountStatementRepository _accountStatementRepository;
        private readonly ITransactionRepository _transactionRepository;

        public GetStatementByIdQueryHandler(
            IAccountStatementRepository accountStatementRepository,
            ITransactionRepository transactionRepository)
        {
            _accountStatementRepository = accountStatementRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<AccountStatementDto?> Handle(GetStatementByIdQuery request, CancellationToken cancellationToken)
        {
            var statement = await _accountStatementRepository.GetByIdAsync(request.Id, cancellationToken);
            if (statement == null)
            {
                return null;
            }

            // Parse month to fetch corresponding transactions
            var parts = statement.StatementMonth.Split('-');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddTicks(-1);

            var transactions = await _transactionRepository.GetTransactionsByCustomerAndPeriodAsync(
                statement.CustomerId, startDate, endDate, cancellationToken);

            return new AccountStatementDto
            {
                Id = statement.Id,
                CustomerId = statement.CustomerId,
                CustomerName = statement.Customer.Name,
                CustomerEmail = statement.Customer.Email,
                StatementMonth = statement.StatementMonth,
                StartingBalance = statement.StartingBalance,
                EndingBalance = statement.EndingBalance,
                GeneratedAt = statement.GeneratedAt,
                EmailSentStatus = statement.EmailSentStatus,
                SentAt = statement.SentAt,
                Transactions = transactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    CustomerId = t.CustomerId,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate
                }).ToList()
            };
        }
    }
}
