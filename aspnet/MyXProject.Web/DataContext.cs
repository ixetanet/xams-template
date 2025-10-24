using Microsoft.EntityFrameworkCore;
using MyXProject.Web.Entities;
using Xams.Core.Base;

namespace MyXProject.Web;

public class DataContext : XamsDbContext<AppUser>
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) 
    {
        base.OnConfiguring(options);
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? 
                               throw new Exception("No Environment variable 'DB_CONNECTION_STRING'");
        options.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasIndex(u => u.EmailAddress)
            .IsUnique();
    }
}