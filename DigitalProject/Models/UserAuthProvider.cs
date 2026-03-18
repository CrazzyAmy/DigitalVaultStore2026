namespace DigitalProject.Models
{
    public class UserAuthProvider
    {
       public int ProviderId { get; set; }

        public int UserId { get; set; }

        public string Provider  { get; set; } = null!;

        public string? ProviderKey { get; set; }

        public string? PasswordHash { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }

}