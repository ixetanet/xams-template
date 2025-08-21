using Microsoft.EntityFrameworkCore;
using Project.Web.Entities;
using Xams.Core.Base;

namespace Project.Web;

public class DataContext : XamsDbContext<AppUser>
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) 
    {
        base.OnConfiguring(options);
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbPath = Path.Join(path, "xamsapp.db");
        options.UseSqlite($"Data Source={dbPath}");
        
        // var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? 
        //                        throw new Exception("No Environment variable 'DB_CONNECTION_STRING'");
        // options.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}