using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebAPIProjectFirst.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace WebAPIProjectFirst.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly string _connectionString;

        public TokenRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task SaveTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt, string userName)
        {
            using var connection = CreateConnection();
            var parameters = new
            {
                UserId = userId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                UserName = userName
            };

            await connection.ExecuteAsync("sp_SaveTokens", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<UserToken> GetTokensAsync(string userId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<UserToken>(
                "sp_GetTokenByUserId",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<string>(
                "sp_RefreshAccessToken",
                new { RefreshToken = refreshToken },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UserToken>> GetExpiringTokensAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<UserToken>(
                "sp_GetExpiringTokens",
                new { ExpiryTime = DateTime.UtcNow.AddMinutes(30) },
                commandType: CommandType.StoredProcedure);
        }
    }
}
