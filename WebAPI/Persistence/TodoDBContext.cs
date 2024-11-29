using Microsoft.EntityFrameworkCore;

namespace WebAPI.Persistence;

public class TodoDbContext(DbContextOptions<TodoDbContext> opt) : DbContext(opt)
{
    public DbSet<Todo> Todos { get; init; }
}
