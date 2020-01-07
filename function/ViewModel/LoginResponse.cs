using System;

namespace Surfrider.PlasticOrigins.Backend.Mobile.ViewModel
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; }
    }
}