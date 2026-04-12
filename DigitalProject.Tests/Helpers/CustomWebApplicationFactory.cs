// CustomWebApplicationFactory.cs
using DigitalProject.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalProject.Tests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private const string DbName = "TestDb";  // ← 固定名稱

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // 1. 移除真實 DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<DigitalVaultStoreDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // 2. 換成記憶體資料庫（固定名稱）
                services.AddDbContext<DigitalVaultStoreDbContext>(options =>
                    options.UseInMemoryDatabase(DbName));  // ← 固定名稱

                // 3. 在這裡直接植入資料！
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider
                    .GetRequiredService<DigitalVaultStoreDbContext>();
                db.Database.EnsureCreated();
                SeedData.Initialize(db);
            });
        }

        public HttpClient CreateClientWithSeedData()
        {
            return CreateClient();  // ← 資料已在 ConfigureWebHost 植入
        }
        public HttpClient CreateClientWithCookies()
        {
            return CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true  // ← 自動保存和帶入 Cookie！
            });
        }
    }
}