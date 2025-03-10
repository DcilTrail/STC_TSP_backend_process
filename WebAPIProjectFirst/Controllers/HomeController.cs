using WebAPIProjectFirst.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;

    public HomeController(TokenService tokenService, IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
    {
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // Get access token for the current user (retrieved from the cookies)
    [HttpGet("access-token")]
    public async Task<IActionResult> GetAccessToken()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized(new { message = "No access token found." });
        }
        return Ok(new { accessToken });
    }

    // Get a valid access token, either from cookies or by refreshing it
    private async Task<string> GetValidAccessTokenAsync()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User ID not found in claims.");
            return null;
        }

        var userName = User.FindFirst(c => c.Type == "nickname")?.Value;
        var tokens = await _tokenService.GetTokensAsync(userId);

        // Check if tokens exist and if the access token is valid
        if (tokens == null || tokens.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning($"Access token expired or not found for UserId: {userId}, attempting refresh.");

            if (tokens?.RefreshToken == null)
            {
                _logger.LogError($"No refresh token available for UserId: {userId}. User must log in again.");
                return null;
            }

            // Refresh the access token using the refresh token
            var newAccessToken = await _tokenService.RefreshAccessTokenAsync(tokens.RefreshToken);
            if (!string.IsNullOrEmpty(newAccessToken))
            {
                var newExpiresAt = DateTime.UtcNow.AddSeconds(3600); // Assuming 1-hour expiry for the new token
                await _tokenService.SaveTokensAsync(userId, newAccessToken, tokens.RefreshToken, newExpiresAt, userName);
                return newAccessToken;
            }

            _logger.LogError("Failed to refresh access token.");
            return null;
        }

        return tokens.AccessToken;
    }

    // Weather forecast example using the access token
    [HttpGet("weather")]
    public async Task<IActionResult> GetWeather()
    {
        var accessToken = await GetValidAccessTokenAsync();
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized(new { message = "No valid access token." });
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Assuming this endpoint is a valid weather API
        var response = await client.GetAsync("https://localhost:5001/WeatherForecast");
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, new { message = "API call failed." });
        }

        var weatherData = await response.Content.ReadAsStringAsync();
        return Ok(new { weatherData });
    }

    // Login endpoint, starts the OAuth2 flow with redirect to the authentication provider
    [HttpGet("login")]
    public IActionResult Login()
    {
        var redirectUri = Url.Action("LoginCallback", "Home", null, Request.Scheme);
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUri }, "Auth0");
    }

    // Callback endpoint after successful login with OAuth provider (e.g., Auth0)
    [HttpGet("login-callback")]
    public async Task<IActionResult> LoginCallback()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
        var expiresAt = DateTime.UtcNow.AddSeconds(3600); // Assuming 1-hour expiry for the new token
        var userName = User.FindFirst(c => c.Type == "nickname")?.Value;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(accessToken))
        {
            // Save the tokens (access and refresh) for the user
            await _tokenService.SaveTokensAsync(userId, accessToken, refreshToken, expiresAt, userName);
        }

        return Ok(new { message = "Login successful", userId, accessToken });
    }

    // Profile endpoint (only accessible when authenticated)
    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        if (!User.Identity?.IsAuthenticated ?? false)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(new { profile = claims });
    }

    // Logout endpoint (signs the user out from the authentication provider)
    [HttpGet("logout")]
    public IActionResult Logout()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" }, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, "Auth0");
    }
}
