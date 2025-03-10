using WebAPIProjectFirst.Services;

public class TokenRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenRefreshService> _logger;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(2);

    public TokenRefreshService(IServiceScopeFactory scopeFactory, ILogger<TokenRefreshService> logger)
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
                var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();

                _logger.LogInformation("Checking for expiring tokens...");

                var expiringTokens = await tokenService.GetExpiringTokensAsync();
                if (expiringTokens.Any()) // Ensure this is returning a proper collection
                {
                    _logger.LogInformation($"Found {expiringTokens.Count()} expiring tokens. Refreshing them...");

                    foreach (var token in expiringTokens)
                    {
                        _logger.LogInformation($"Attempting to refresh token for UserId: {token.UserId}");

                        var refreshedToken = await tokenService.RefreshAccessTokenAsync(token.RefreshToken);

                        if (!string.IsNullOrEmpty(refreshedToken))
                        {
                            _logger.LogInformation($"Successfully refreshed token for UserId: {token.UserId}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to refresh token for UserId: {token.UserId}. It may be invalid.");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No tokens are expiring soon.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during token refresh operation: {ex.Message}");
            }

            await Task.Delay(_refreshInterval, stoppingToken);
        }
    }
}
