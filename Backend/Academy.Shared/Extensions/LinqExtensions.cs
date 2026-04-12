using System.Linq.Expressions;
using System.Reflection;

namespace Academy.Shared.Extensions
{
    public static class LinqExtensions
    {
        public static IOrderedQueryable<T> DynamicOrderBy<T>(this IQueryable<T> query, string orderByMember, bool orderByDescending)
        {
            var entityType = typeof(T);
            var property = entityType.GetProperty(orderByMember, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
            {
                throw new ArgumentException($"No property '{orderByMember}' found on type '{entityType.Name}'");
            }

            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            MethodCallExpression resultExp = Expression.Call(
                typeof(Queryable),
                orderByDescending ? "OrderByDescending" : "OrderBy",
                [entityType, property.PropertyType],
                query.Expression,
                Expression.Quote(orderByExp)
            );

            return (IOrderedQueryable<T>)query.Provider.CreateQuery<T>(resultExp);
        }

        public static Expression<Func<T, bool>> DynamicFilterBy<T>(string propertyName, object value, ExpressionType comparisonType)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, propertyName);
            var constant = Expression.Constant(value);
            var comparison = Expression.MakeBinary(comparisonType, member, constant);

            return Expression.Lambda<Func<T, bool>>(comparison, parameter);
        }

        public static Expression<Func<T, bool>> StartsWith<T>(string propertyName, string prefix)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var constant = Expression.Constant(prefix, typeof(string));

            // Handle null property values to avoid exceptions
            var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
            var startsWithCall = Expression.Call(property, method, constant);
            var combinedExpression = Expression.AndAlso(nullCheck, startsWithCall);

            return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        public static Expression<Func<T, bool>> Contains<T>(string propertyName, string prefix)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var constant = Expression.Constant(prefix, typeof(string));

            // Handle null property values to avoid exceptions
            var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
            var containsCall = Expression.Call(property, method, constant);
            var combinedExpression = Expression.AndAlso(nullCheck, containsCall);

            return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        public static Expression<Func<T, bool>> Equals<T>(string propertyName, string prefix)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var method = typeof(string).GetMethod("Equals", new[] { typeof(string) });
            var constant = Expression.Constant(prefix, typeof(string));

            // Handle null property values to avoid exceptions
            var nullCheck = Expression.NotEqual(property, Expression.Constant(null));
            var containsCall = Expression.Call(property, method, constant);
            var combinedExpression = Expression.AndAlso(nullCheck, containsCall);

            return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }
    }
}
