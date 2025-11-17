using Ater.AspNetCore.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Share.Exceptions;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, Localizer localizer)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (DbUpdateConcurrencyException)
        {
            // 并发冲突提示
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(Localizer.AlreadyUpdated), ctx.TraceIdentifier)
            );
        }
        catch (DbUpdateException ex) when (EfCoreErrorHelper.IsUniqueConstraintViolation(ex))
        {
            // 唯一约束冲突提示
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;

            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(Localizer.ConflictResource), ctx.TraceIdentifier)
            );
        }
        catch (DbUpdateException ex)
        {
            // 其他数据库错误
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(ex.Message, ctx.TraceIdentifier, "database error!")
            );
        }
        catch (BusinessException ex)
        {
            ctx.Response.StatusCode = ex.StatusCodes;
            await ctx.Response.WriteAsJsonAsync(
                new ErrorResult(localizer.Get(ex.ErrorCode), ctx.TraceIdentifier, ex.ErrorCode)
            );
        }
        catch (Exception ex)
        {
            // 非数据库类异常
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(new ErrorResult(ex.Message, ctx.TraceIdentifier));
        }
    }
}

public static class EfCoreErrorHelper
{
    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            // SQL Server: 2627=主键冲突, 2601=唯一约束冲突
            return sqlEx.Number is 2627 or 2601;
        }
        if (ex.InnerException is PostgresException pgEx)
        {
            // PostgreSQL: 23505=unique_violation
            return pgEx.SqlState == "23505";
        }
        return false;
    }
}
