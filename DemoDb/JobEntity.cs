namespace DemoDb;

public class JobEntity
{
    public int Id { get; set; }

    public string Name { get; set; }
    public int Salary { get; set; }
    
    public override string ToString() => $"{Name} ({Salary})";
}