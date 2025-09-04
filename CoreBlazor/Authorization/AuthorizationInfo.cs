namespace CoreBlazor.Authorization;

public record AuthorizationInfo(DbContextAction ContextAction, string DbContextName, string DbSetName);
