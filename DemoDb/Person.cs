using System.ComponentModel.DataAnnotations;

namespace DemoDb;

public class Person
{
    [Required] public string Name { get; set; } = null!;

    [Required]
    public DateTime BornAt { get; set; }
    
    public Gender Gender { get; set; }
}