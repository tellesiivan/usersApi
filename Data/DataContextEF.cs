using DotnetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Data;

public class DataContextEF : DbContext
{
    private readonly IConfiguration _configuration;

    public DataContextEF(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserSalary> UserSalary { get; set; }
    public virtual DbSet<UserJobInfo> UsersJobInfo { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }
        ;

        optionsBuilder.UseSqlServer(
            _configuration.GetConnectionString("DefaultConnection"),
            options => options.EnableRetryOnFailure()
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TutorialAppSchema");

        /*
        --> Users Table
        */
        // --> What table to target and which schema to follow
        modelBuilder
            .Entity<User>()
            .ToTable("Users", "TutorialAppSchema")
            // --> Let's entrity know which key on our table is our primary key
            .HasKey((user) => user.UserId);

        /*
        --> UserSalary Table
        */
        modelBuilder.Entity<UserSalary>().HasKey((user) => user.UserId);

        /*
        --> UserJobInfo Table
        */
        modelBuilder
            .Entity<UserJobInfo>()
            .ToTable("UserJobInfo", "TutorialAppSchema")
            .HasKey((user) => user.UserId);
    }
}
