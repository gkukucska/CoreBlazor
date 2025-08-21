using System.ComponentModel.DataAnnotations;

namespace DemoDb;

public class JobEntity
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
    public int Salary { get; set; }
}