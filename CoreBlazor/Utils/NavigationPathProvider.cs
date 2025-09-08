using CoreBlazor.Interfaces;

namespace CoreBlazor.Utils;

internal class DefaultNavigationPathProvider : INavigationPathProvider
{
    public string GetPathToCreateEntity(string dbContextName, string dbSetName)
        => $"/DbContext/{dbContextName}/DbSet/{dbSetName}/Create";

    public string GetPathToDeleteEntity(string dbContextName, string dbSetName, string entityId)
        => $"/DbContext/{dbContextName}/DbSet/{dbSetName}/Delete/{entityId}";

    public string GetPathToEditEntity(string dbContextName, string dbSetName, string entityId)
        => $"/DbContext/{dbContextName}/DbSet/{dbSetName}/Edit/{entityId}";

    public string GetPathToReadDbContextInfo(string dbContextName) 
        => $"/DbContext/{dbContextName}/Info";

    public string GetPathToReadEntities(string dbContextName, string dbSetName)
        => $"/DbContext/{dbContextName}/DbSet/{dbSetName}";
}
