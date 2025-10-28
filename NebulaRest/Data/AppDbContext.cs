using Microsoft.EntityFrameworkCore;
using NebulaRest.Entities;

namespace NebulaRest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
