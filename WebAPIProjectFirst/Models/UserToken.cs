using System;
using System.ComponentModel.DataAnnotations;


namespace WebAPIProjectFirst.Models
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string UserName { get; set; }

    }
}
