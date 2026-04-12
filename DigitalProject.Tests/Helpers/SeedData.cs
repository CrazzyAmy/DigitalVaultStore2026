// Helpers/SeedData.cs
using DigitalProject.Data;
using DigitalProject.Domain;
using DigitalProject.Models;

namespace DigitalProject.Tests.Helpers
{
    public static class SeedData
    {
        public static void Initialize(DigitalVaultStoreDbContext db)
        {
            // 只種 Roles，不種 User
            // User 透過 Register API 建立，確保密碼 Hash 正確

            if (db.Roles.Any()) return;  // 避免重複植入

            var userRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Name = "一般使用者",
                Code = "user",
                CreatedAt = DateTime.UtcNow
            };
            var adminRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Name = "系統管理員",
                Code = "admin",
                CreatedAt = DateTime.UtcNow
            };
            var managerRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Name = "商品管理員",
                Code = "manager",
                CreatedAt = DateTime.UtcNow
            };
            var supportRole = new Role
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
                Name = "客服人員",
                Code = "support",
                CreatedAt = DateTime.UtcNow
            };

            db.Roles.AddRange(userRole, adminRole, managerRole, supportRole);
            db.SaveChanges();
        }
    }
}