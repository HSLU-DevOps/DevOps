using Microsoft.EntityFrameworkCore;
using WebAPI.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoDbContext>(opt =>
{
    opt.UseNpgsql($"User ID={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                  $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};" +
                  $"Host=postgres;" +  // Use 'postgres' to reference the Docker service
                  $"Port=5432;" +
                  $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
                  $"Pooling=true;" +
                  $"Minimum Pool Size=0;" +
                  $"Maximum Pool Size=100;" +
                  $"Connection Lifetime=0;");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();