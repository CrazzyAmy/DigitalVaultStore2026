

using DigitalProject.Domain;

namespace DigitalProject.Models
{
    public partial class UserAuthProvider
    {
        public Guid Id { get; set; }

        public int UserId { get; set; }


        public AuthProvider Provider { get; set; } 

        public string? ProviderKey { get; set; }

        public string? PasswordHash { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }

}