using Microsoft.EntityFrameworkCore;
using PinkNightmares.Models;

namespace PinkNightmares.Repositories;

public class PinkDb(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;
}