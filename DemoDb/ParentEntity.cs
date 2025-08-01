namespace DemoDb;

public class ParentEntity : Person
{
    
    public Guid Id { get; set; }
    
    public List<ChildEntity> Children { get; set; } = new List<ChildEntity>();

    public JobEntity Job { get; set; }
}