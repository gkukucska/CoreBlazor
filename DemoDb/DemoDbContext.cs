using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Bogus;

namespace DemoDb;

public class DemoDbContext : DbContext
{
    public DbSet<Person> People { get; set; }


    public DbSet<Job> Jobs { get; set; }
    public DbSet<ImageData> Images { get; set; }

    public DemoDbContext() : base()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("FamilyDb");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Generate fake data

        var jobFaker = new Faker<Job>()
            .RuleFor(j => j.Id, f => f.IndexFaker + 1)
            .RuleFor(j => j.Name, f => f.Name.JobTitle())
            .RuleFor(j => j.Salary, f => f.Random.Int(30000, 150000));

        var jobs = jobFaker.Generate(500);
        modelBuilder.Entity<Job>().HasData(jobs);

        var personFaker = new Faker<Person>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FullName())
            .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
            .RuleFor(p => p.BornAt, f => f.Date.PastDateOnly(30))
            .RuleFor(p => p.JobId, f => f.PickRandom(jobs.Select(j => j.Id)));
        var people = personFaker.Generate(50_000);
        modelBuilder.Entity<Person>().HasData(people);

    }
}