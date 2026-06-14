using AccountStatements.Domain.Entities;

namespace AccountStatements.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Customer>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<List<Customer>> GetActivePagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
    }
}
