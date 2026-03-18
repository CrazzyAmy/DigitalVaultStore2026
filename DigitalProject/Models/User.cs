using DigitalProject.Domain;

namespace DigitalProject.Models
{
    public partial class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string? AvatarUrl { get; set; }

        public UserRole Role { get; set; } = UserRole.user;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public ICollection<UserAuthProvider> UserAuthProviders { get; set; } = new List<UserAuthProvider>();

        public ICollection<Order>Orders { get; set; } = new List<Order>();  

    }
}
