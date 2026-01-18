namespace CoreBlazor;

public class DiscoveredContext
{
    public required Type ContextType { get; set; }
    public required List<DiscoveredSet> Sets { get; set; }
}