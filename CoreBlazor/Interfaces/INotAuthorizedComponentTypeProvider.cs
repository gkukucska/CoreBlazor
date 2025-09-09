using Microsoft.EntityFrameworkCore;

namespace CoreBlazor.Interfaces;

public interface INotAuthorizedComponentTypeProvider
{
    Type GetNotAuthorizedComponentType<TContext,TEntity>() where TContext: DbContext where TEntity: class;
}
