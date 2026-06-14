using System.Linq.Expressions;

namespace AccountStatements.Application.Interfaces
{
    public interface IBackgroundJobService
    {
        void Enqueue<T>(Expression<Func<T, Task>> methodCall);
    }
}
