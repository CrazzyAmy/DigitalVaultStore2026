using DigitalProject.Domain;
namespace DigitalProject.Models
{
    public partial class UserAuthProvider
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }  
        public AuthProvider Provider { get; set; }
        public string? ProviderKey { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }  

        public User User { get; set; } = null!;
    }
}