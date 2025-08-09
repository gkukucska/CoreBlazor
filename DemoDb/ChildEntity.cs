namespace DemoDb;

public class ChildEntity : Person
{
    public Guid Id { get; set; }

    public Guid FatherId { get; set; }

    public ParentEntity Father { get; set; } = null!;
    public Guid MotherId { get; set; }
    public ParentEntity Mother { get; set; } = null!;

}