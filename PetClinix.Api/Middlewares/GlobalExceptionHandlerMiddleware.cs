using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace PetClinix.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocorreu um erro não tratado na aplicação.");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case DbUpdateException dbEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var message = dbEx.InnerException?.Message ?? dbEx.Message;

                if (message.Contains("duplicate key"))
                    message = "Já existe um registro com esses dados.";
                else if (message.Contains("violates foreign key"))
                    message = "Referência a um registro que não existe.";
                else
                    message = "Os dados enviados violam as regras do banco de dados.";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    statusCode = context.Response.StatusCode,
                    message = message
                }));
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    statusCode = context.Response.StatusCode,
                    message = "Ocorreu um erro interno no servidor. Tente novamente mais tarde."
                }));
                break;
        }
    }
}