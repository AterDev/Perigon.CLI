using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ServiceDefaults.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (DbUpdateException ex) when (EfCoreErrorHelper.IsUniqueConstraintViolation(ex))
        {
            // 唯一约束冲突提示
            ctx.Response.StatusCode = StatusCodes.Status409Conflict; // 409 冲突
            await ctx.Response.WriteAsJsonAsync(
                new { code = "DATA_CONFLICT", message = "数据已存在，请检查后重试" }
            );
        }
        catch (DbUpdateException)
        {
            // 其他数据库错误
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new { code = "DB_ERROR", message = "数据库操作失败，请稍后重试" }
            );
        }
        catch (Exception)
        {
            // 非数据库类异常
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await ctx.Response.WriteAsJsonAsync(
                new { code = "SERVER_ERROR", message = "服务器发生错误，请稍后重试" }
            );
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
            return sqlEx.Number == 2627 || sqlEx.Number == 2601;
        }
        if (ex.InnerException is PostgresException pgEx)
        {
            // PostgreSQL: 23505=unique_violation
            return pgEx.SqlState == "23505";
        }
        return false;
    }
}
