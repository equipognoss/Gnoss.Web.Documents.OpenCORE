using Es.Riam.Gnoss.Util.General;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Gnoss.Web.Documents.Middlewares
{
    public class ApplicationErrorMiddleware
    {
        private RequestDelegate _next;

        public ApplicationErrorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, LoggingService loggingService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(loggingService, context, ex);
            }
        }

        protected Task HandleExceptionAsync(LoggingService error, HttpContext context, Exception ex)
        {
            // Code that runs when an unhandled error occurs
            error.GuardarLogError(ex);

            var code = HttpStatusCode.InternalServerError;

            //if (ex is ParametersNotConfiguredException)
            //{
            //    code = HttpStatusCode.BadRequest;
            //    Log.Information($"{ex.Message}\n");
            //}
            //else if (ex is FailedLoadConfigJsonException)
            //{
            //    code = HttpStatusCode.InternalServerError;
            //    Log.Information($"{ex.Message}\n");
            //}

            var result = JsonConvert.SerializeObject(new { error = "Internal server error" });
            if (code != HttpStatusCode.InternalServerError)
            {
                result = JsonConvert.SerializeObject(new { error = ex.Message });
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;


            // Clear the error from the server
            //Server.ClearError();
            return context.Response.WriteAsync(result);
        }
    }

    public static class ApplicationErrorExtensions
    {
        public static IApplicationBuilder UseApplicationErrorMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApplicationErrorMiddleware>();
        }
    }
}
