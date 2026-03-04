using Microsoft.EntityFrameworkCore;
using PinkNightmares.Models;

namespace PinkNightmares.Repositories;

public class PinkDb(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(mb =>
        {
            mb.Property(p => p.Name).IsRequired().HasMaxLength(100);
            mb.Property(p => p.Email).IsRequired().HasMaxLength(100);
            mb.Property(p => p.Password).IsRequired(false);
        });
        base.OnModelCreating(modelBuilder);
        
    }
}

