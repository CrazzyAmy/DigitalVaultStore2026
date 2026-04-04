using DigitalProject.Data;
using DigitalProject.Interface;
using DigitalProject.Interface.Auth;
using DigitalProject.Interface.Category;
using DigitalProject.Interface.Orders;
using DigitalProject.Interface.Payment;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Interface.Reviews;
using DigitalProject.Interface.User;
using DigitalProject.Middleware;
using DigitalProject.Repositories;
using DigitalProject.Repositories.Payment;
using DigitalProject.Repositories.Prouduct;
using DigitalProject.Repositories.Reviews;
using DigitalProject.Security;
using DigitalProject.Services;
using DigitalProject.Services.Payment;
using DigitalProject.Services.Prouduct;
using DigitalProject.Services.Reviews;
using DigitalProject.Services.User;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<DigitalVaultStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

// ── Repositories ──────────────────────────────────────────────────────────────
// Program.cs
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();


// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentServie, PaymentService>();

// ── Security ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── JWT ───────────────────────────────────────────────────────────────────────

var jwtSettings = builder.Configuration.GetSection("JwtTokenSettings");



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "Cookies"; 
})
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings["IssuerSigningKey"]!)),
            ClockSkew = TimeSpan.Zero,
        };
        options.Events = new JwtBearerEvents
        {
            OnChallenge = context =>
            {
                options.IncludeErrorDetails = false;
                context.HandleResponse();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 401;
                var body = new
                {
                    error = "Unauthorized",
                    error_description = "Authentication failed"
                };
                return context.Response.WriteAsync(JsonSerializer.Serialize(body));
            }
        };
    })
  .AddGoogle(options =>
   {
       options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
       options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
       options.CallbackPath = "/signin-google";
       options.SignInScheme = "Cookies";
   });



builder.Services.AddAuthorization();

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DigitalProject API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "輸入 JWT Token，格式：Bearer {token}",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = []
    });
}); // ← SwaggerGen 在這裡結束

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build(); // ← 移到這裡
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();