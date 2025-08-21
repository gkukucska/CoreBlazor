using System.ComponentModel.DataAnnotations;

namespace DemoDb;

public class Person
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Required]
    public DateTime BornAt { get; set; }
    
    public Gender Gender { get; set; }

    public int JobId { get; set; }
    public JobEntity? Job { get; set; } 
    public override string ToString() => Name;
}