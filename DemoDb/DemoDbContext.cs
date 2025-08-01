using Microsoft.EntityFrameworkCore;

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
}