using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";

        _logger.LogInformation("🚀 [Start] Handling {RequestName} from User: {UserId}", requestName, userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                _logger.LogWarning("⚠️ [Long Running] {RequestName} took {ElapsedMilliseconds} ms to complete for User: {UserId}",
                    requestName, elapsedMilliseconds, userId);
            }
            else
            {
                _logger.LogInformation("✅ [Success] {RequestName} handled in {ElapsedMilliseconds} ms", 
                    requestName, elapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ [Error] {RequestName} failed after {ElapsedMilliseconds} ms for User: {UserId}", 
                requestName, stopwatch.ElapsedMilliseconds, userId);
            throw;
        }
    }
}
