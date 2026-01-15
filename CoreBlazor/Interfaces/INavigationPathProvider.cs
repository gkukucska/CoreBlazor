namespace CoreBlazor.Interfaces
{
    public interface INavigationPathProvider
    {
        string GetPathToReadDbContextInfo(string dbContextName);
        string GetPathToReadEntities(string dbContextName, string dbSetName);
        string GetPathToCreateEntity(string dbContextName, string dbSetName);
        string GetPathToEditEntity(string dbContextName, string dbSetName, string entityId);
        string GetPathToDeleteEntity(string dbContextName, string dbSetName, string entityId);
    }
}
