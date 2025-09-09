using CoreBlazor.Components;
using CoreBlazor.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Utils;

public class DefaultNotAuthorizedComponentTypeProvider : INotAuthorizedComponentTypeProvider
{
    public Type GetNotAuthorizedComponentType<TContext,TEntity>() where TContext: DbContext where TEntity: class
    {
        return typeof(NotAuthorizedComponent<TContext,TContext>);
    }
}
