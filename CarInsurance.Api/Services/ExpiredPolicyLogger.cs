using CarInsurance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services
{
    public class ExpiredPolicyLogger : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredPolicyLogger> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);
        private readonly HashSet<long> _processedPolicyIds = new HashSet<long>();

        public ExpiredPolicyLogger(IServiceScopeFactory scopeFactory, ILogger<ExpiredPolicyLogger> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var today = DateOnly.FromDateTime(DateTime.UtcNow);

                    var expiredPolicies = await db.Policies
                        .Where(p => p.EndDate < today && p.IsLogged == false)
                        .ToListAsync();

                    foreach (var policy in expiredPolicies)
                    {
                        _logger.LogInformation("Policy {PolicyId} has expired on {EndDate}", policy.Id, policy.EndDate);
                        _processedPolicyIds.Add(policy.Id);
                        policy.IsLogged = true;
                    }
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ExpiredPolicyLogger");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

    }
}