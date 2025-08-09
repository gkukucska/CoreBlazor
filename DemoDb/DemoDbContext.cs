using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace DemoDb;

public class DemoDbContext : DbContext
{
    public DbSet<ParentEntity> Parents { get; set; }

    public DbSet<ChildEntity> Children { get; set; }

    public DbSet<JobEntity> Jobs { get; set; }
    

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("DemoDb");
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ParentEntity>()
            .HasMany(e => e.FatheredChildren)
            .WithOne(e => e.Father)
            .HasForeignKey(e => e.FatherId)
            .IsRequired();
        modelBuilder.Entity<ParentEntity>()
            .HasMany(e => e.MotheredChildren)
            .WithOne(e => e.Mother)
            .HasForeignKey(e => e.MotherId)
            .IsRequired();
    }
}