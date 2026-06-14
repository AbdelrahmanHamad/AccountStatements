using System.Linq.Expressions;
using AccountStatements.Application.Interfaces;
using Hangfire;

namespace AccountStatements.Infrastructure.Services
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public HangfireBackgroundJobService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            _backgroundJobClient.Enqueue(methodCall);
        }
    }
}