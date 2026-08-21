using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.BackgroundServices;

public class ExpiredCardsCheckWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredCardsCheckWorker> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<ExpiredCardsCheckWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Checking for overdue tasks in the background...");

            // BackgroundService is a Singleton, so to work with a scoped DbContext, we create a scope
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            // Find the cards for which the deadline has passed
            var now = DateTime.UtcNow;
            var expiredCardsCount = context.Cards
                .Count(c => c.DueDate.HasValue && c.DueDate.Value < now);

            if (expiredCardsCount > 0)
            {
                _logger.LogWarning("{Count} overdue tasks found!", expiredCardsCount);
            }

        // Run a check every hour
        await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}
