using System.Reflection;

namespace EntityFramework;

public static class Extensions
{
    public static async Task<int> PartialUpdateAsync<TEntity, TUpdateDto>(
        this DbContext db,
        Guid id,
        TUpdateDto dto,
        bool updateUpdatedTime = true
    )
        where TEntity : EntityBase
        where TUpdateDto : class
    {
        DbSet<TEntity> set = db.Set<TEntity>();

        ParameterExpression eParam = Expression.Parameter(typeof(TEntity), "e");
        MemberExpression keyProp = Expression.Property(eParam, nameof(EntityBase.Id));
        ConstantExpression idConst = Expression.Constant(id, typeof(Guid));
        BinaryExpression equal = Expression.Equal(keyProp, idConst);
        var whereLambda = Expression.Lambda<Func<TEntity, bool>>(equal, eParam);

        // 构造 ExecuteUpdate 的 lambda
        return await set.Where(whereLambda)
            .ExecuteUpdateAsync(updater =>
            {
                foreach (PropertyInfo dtoProp in typeof(TUpdateDto).GetProperties())
                {
                    var value = dtoProp.GetValue(dto);
                    if (value is null)
                    {
                        continue;
                    }

                    PropertyInfo? entityPropInfo = typeof(TEntity).GetProperty(dtoProp.Name);
                    if (entityPropInfo is null)
                    {
                        continue;
                    }

                    // e => e.Prop
                    MemberExpression eProp = Expression.Property(eParam, entityPropInfo);
                    var eLambda = Expression.Lambda<Func<TEntity, object>>(
                        Expression.Convert(eProp, typeof(object)),
                        eParam
                    );

                    updater = updater.SetProperty(eLambda, value);
                }

                // 如果 updateUpdatedTime 为 true，则更新 UpdatedTime
                if (updateUpdatedTime)
                {
                    MemberExpression updatedTimeEProp = Expression.Property(
                        eParam,
                        nameof(EntityBase.UpdatedTime)
                    );
                    var updatedTimeLambda = Expression.Lambda<Func<TEntity, object>>(
                        Expression.Convert(updatedTimeEProp, typeof(object)),
                        eParam
                    );
                    updater = updater.SetProperty(updatedTimeLambda, DateTimeOffset.UtcNow);
                }
            });
    }
}
