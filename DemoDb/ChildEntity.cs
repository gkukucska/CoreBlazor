namespace DemoDb;

public class ChildEntity : Person
{
    public Guid Id { get; set; }

    public ParentEntity Father { get; set; }
    public ParentEntity Mother { get; set; }

}