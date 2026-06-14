using MediatR;
using AccountStatements.Domain.Entities;
using AccountStatements.Domain.Repositories;

namespace AccountStatements.Application.Features.Seed.Commands.SeedData
{
    public record SeedDataCommand : IRequest<string>;

    public class SeedDataCommandHandler : IRequestHandler<SeedDataCommand, string>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SeedDataCommandHandler(
            ICustomerRepository customerRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> Handle(SeedDataCommand request, CancellationToken cancellationToken)
        {

            var existing = await _customerRepository.GetAllActiveAsync(cancellationToken);
            if (existing.Count > 0)
            {
                return "Database already contains data. Seeding skipped.";
            }

            var cust1 = new Customer { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Alice Johnson", Email = "abdelrahman.hamad2003@gmail.com" };
            var cust2 = new Customer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "John Doe", Email = "john.doe@example.com" };
            var cust3 = new Customer { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Jane Smith", Email = "jane.smith@example.com" };

            await _customerRepository.AddAsync(cust1, cancellationToken);
            await _customerRepository.AddAsync(cust2, cancellationToken);
            await _customerRepository.AddAsync(cust3, cancellationToken);


            var t1_1 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = 5000.00m, Description = "Salary Deposit", TransactionDate = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc) };
            var t1_2 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = -120.50m, Description = "Grocery Shopping", TransactionDate = new DateTime(2026, 5, 10, 14, 30, 0, DateTimeKind.Utc) };
            var t1_3 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = -50.00m, Description = "Gas Station", TransactionDate = new DateTime(2026, 5, 25, 18, 0, 0, DateTimeKind.Utc) };
            
            var t1_4 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = 5000.00m, Description = "Salary Deposit", TransactionDate = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc) };
            var t1_5 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = -300.00m, Description = "Electronics Store", TransactionDate = new DateTime(2026, 6, 5, 11, 0, 0, DateTimeKind.Utc) };
            var t1_6 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust1.Id, Amount = -75.25m, Description = "Restaurant Dining", TransactionDate = new DateTime(2026, 6, 12, 21, 15, 0, DateTimeKind.Utc) };


            var t2_1 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust2.Id, Amount = 3000.00m, Description = "Consulting Fee", TransactionDate = new DateTime(2026, 5, 5, 10, 0, 0, DateTimeKind.Utc) };
            var t2_2 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust2.Id, Amount = -1500.00m, Description = "Rent Payment", TransactionDate = new DateTime(2026, 5, 6, 12, 0, 0, DateTimeKind.Utc) };

            var t2_3 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust2.Id, Amount = 3200.00m, Description = "Consulting Fee", TransactionDate = new DateTime(2026, 6, 5, 10, 0, 0, DateTimeKind.Utc) };
            var t2_4 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust2.Id, Amount = -80.00m, Description = "Pharmacy", TransactionDate = new DateTime(2026, 6, 8, 15, 0, 0, DateTimeKind.Utc) };


            var t3_1 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust3.Id, Amount = 10000.00m, Description = "Initial Account Funding", TransactionDate = new DateTime(2026, 6, 10, 9, 30, 0, DateTimeKind.Utc) };
            var t3_2 = new Transaction { Id = Guid.NewGuid(), CustomerId = cust3.Id, Amount = -250.00m, Description = "Utility Bills", TransactionDate = new DateTime(2026, 6, 11, 16, 45, 0, DateTimeKind.Utc) };

            var list = new List<Transaction> { t1_1, t1_2, t1_3, t1_4, t1_5, t1_6, t2_1, t2_2, t2_3, t2_4, t3_1, t3_2 };
            foreach (var t in list)
            {
                await _transactionRepository.AddAsync(t, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return "Database successfully seeded with 3 customers and 12 sample transactions.";
        }
    }
}
