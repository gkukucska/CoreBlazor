namespace DemoDb;

public class ParentEntity : Person
{
    
    public Guid Id { get; set; }

    public ICollection<ChildEntity> FatheredChildren { get; set; } = [];

    public ICollection<ChildEntity> MotheredChildren { get; set; } = [];

    public JobEntity Job { get; set; }
}