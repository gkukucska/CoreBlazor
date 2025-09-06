namespace CoreBlazor.Authorization;

public static class Policies
{
    public const string Info = nameof(Info);
    public const string Read = nameof(Read);
    public const string Create = nameof(Create);
    public const string Edit = nameof(Edit);
    public const string Delete = nameof(Delete);
    public static string CanReadInfo(Type contextType) => $"{contextType.Name}/{Info}";
    public static string CanCreate(Type contextType, Type entityType) => $"{contextType.Name}/{entityType.Name}/{Create}";
    public static string CanRead(Type contextType, Type entityType) => $"{contextType.Name}/{entityType.Name}/{Read}";
    public static string CanEdit(Type contextType, Type entityType) => $"{contextType.Name}/{entityType.Name}/{Edit}";
    public static string CanDelete(Type contextType, Type entityType) => $"{contextType.Name}/{entityType.Name}/{Delete}";
}

public static class Policies<TContext>
{
    public static string CanReadInfo => Policies.CanReadInfo(typeof(TContext));
}

public static class Policies<TContext,TEntity>
{
    public static string CanCreate => Policies.CanCreate(typeof(TContext),typeof(TEntity));
    public static string CanRead => Policies.CanRead(typeof(TContext), typeof(TEntity));
    public static string CanEdit => Policies.CanEdit(typeof(TContext), typeof(TEntity));
    public static string CanDelete => Policies.CanDelete(typeof(TContext), typeof(TEntity));
}
