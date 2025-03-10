using Microsoft.AspNetCore.Mvc;
using WebAPIProjectFirst.Models;
using WebAPIProjectFirst.Services;
using System.Threading.Tasks;
using System;

namespace WebAPIProjectFirst.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public TokenController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        // POST api/token
        [HttpPost]
        public async Task<IActionResult> SaveOrUpdateToken([FromBody] TokenRequest request)
        {
            try
            {
                await _tokenService.SaveTokensAsync(request.UserId, request.AccessToken, request.RefreshToken, request.ExpiresAt, request.UserName);
                return Ok("Token saved or updated successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // GET api/token/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetToken(string userId)
        {
            try
            {
                var token = await _tokenService.GetTokensAsync(userId);
                if (token == null)
                {
                    return NotFound("Token not found.");
                }
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // POST api/token/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var newAccessToken = await _tokenService.RefreshAccessTokenAsync(refreshToken);
                if (string.IsNullOrEmpty(newAccessToken))
                {
                    return Unauthorized("Failed to refresh the token.");
                }
                return Ok(new { AccessToken = newAccessToken });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // GET api/token/expiring
        [HttpGet("expiring")]
        public async Task<IActionResult> GetExpiringTokens()
        {
            try
            {
                var expiringTokens = await _tokenService.GetExpiringTokensAsync();
                return Ok(expiringTokens);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
