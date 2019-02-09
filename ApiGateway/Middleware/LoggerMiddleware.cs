using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NLog.Fluent;

namespace ApiGateway.Middleware
{
    class LoggerMiddleware
    {
        private const string MessageTemplate =
            "{0} : HTTP {1} {2} responded {3} in {4} ms";

        private enum LogEventLevel
        {
            Error, Information
        }

        private readonly RequestDelegate _next;

        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            var sw = Stopwatch.StartNew();
            try
            {
                await _next(httpContext);
                sw.Stop();

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;

                Console.WriteLine(MessageTemplate, level, httpContext.Request.Method, httpContext.Request.Path, statusCode, sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex) when (LogException(httpContext, sw, ex)) { }
        }

        private static bool LogException(HttpContext httpContext, Stopwatch sw, Exception ex)
        {
            sw.Stop();

            Console.WriteLine(ex.ToString() + " in " + sw.Elapsed.TotalMilliseconds + " ms");

            return false;
        }
    }
}