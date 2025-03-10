using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPIProjectFirst.Models;
using WebAPIProjectFirst.Repositories;

namespace WebAPIProjectFirst.Services
{
    public class TokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly ILogger<TokenService> _logger;

        public TokenService(ITokenRepository tokenRepository, ILogger<TokenService> logger)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
        }

        public async Task SaveTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt, string userName)
        {
            await _tokenRepository.SaveTokensAsync(userId, accessToken, refreshToken, expiresAt, userName);
        }

        public async Task<UserToken> GetTokensAsync(string userId)
        {
            return await _tokenRepository.GetTokensAsync(userId);
        }

        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            _logger.LogInformation($"Refreshing access token using refresh token: {refreshToken}");
            return await _tokenRepository.RefreshAccessTokenAsync(refreshToken);
        }

        public async Task<IEnumerable<UserToken>> GetExpiringTokensAsync()
        {
            return await _tokenRepository.GetExpiringTokensAsync();
        }
    }
}
