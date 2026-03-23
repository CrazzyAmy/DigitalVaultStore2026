using DigitalProject.Data;
using DigitalProject.Interface;
using DigitalProject.Interface.Categoy;
using DigitalProject.Interface.Prouduct;
using DigitalProject.Repositories;
using DigitalProject.Repositories.Prouduct;
using DigitalProject.Services;
using DigitalProject.Services.Prouduct;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DigitalVaultStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbContext")));

// Add services to the container.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
