using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }

            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = (int) HttpStatusCode.InternalServerError;


                var response = _env.IsDevelopment()
                    ? new ApiException((int) HttpStatusCode.InternalServerError, 
                        e.Message, 
                        e.StackTrace.ToString())
                    : new ApiException((int) HttpStatusCode.InternalServerError);

                var jsonOptions = new JsonSerializerOptions{
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, jsonOptions);
                await ctx.Response.WriteAsync(json);

            }
        }
    }
}