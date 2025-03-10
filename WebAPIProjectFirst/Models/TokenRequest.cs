namespace WebAPIProjectFirst.Models
{
    public class TokenRequest
    {
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; }
    }
}
