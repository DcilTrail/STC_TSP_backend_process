using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPIProjectFirst.Models;

namespace WebAPIProjectFirst.Repositories
{
    public interface ITokenRepository
    {
        Task SaveTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt, string userName);
        Task<UserToken> GetTokensAsync(string userId);
        Task<string> RefreshAccessTokenAsync(string refreshToken);
        Task<IEnumerable<UserToken>> GetExpiringTokensAsync();
    }
}
