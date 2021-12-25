using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoWebServiceMethodRepository : IPiwigoWebServiceMethodRepository
    {
        private readonly IServiceProvider _serviceProvider;
        public PiwigoWebServiceMethodRepository(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
        public IEnumerable<string> GetMethods() => _serviceProvider.GetServices<IPiwigoWebServiceMethod>().Select(x => x.MethodName);
        public IPiwigoWebServiceMethod GetMethod(string methodName) => _serviceProvider.GetServices<IPiwigoWebServiceMethod>().SingleOrDefault(x => x.MethodName == methodName);

        public Uri GetMethodUri(string methodName, IReadOnlyDictionary<string, IConvertible> parameters)
        {
            GetMethod(methodName);
            return new Uri($"/ws.php?method={methodName}&{string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"))}", UriKind.Relative);
        }

        public async Task ExecuteMethod(IPiwigoWebServiceMethod method, IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            var context = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var logger = _serviceProvider.GetRequiredService<ILogger<PiwigoWebServiceMethodRepository>>();

            async Task Deny()
            {
                context.Response.StatusCode = 401;
                await WriteJsonResult(new PiwigoResponse { Stat = "fail", Error = 401, Message = "Authentication required" });
            }

            uint? userId = null;
            if (!method.AllowAnonymous)
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await Deny();
                    return;
                }

                userId = context.User.Identity.GetUserId();
            }

            RouteData routeData = context.GetRouteData();
            ActionDescriptor actionDescriptor = new();
            ActionContext actionContext = new(context, routeData, actionDescriptor);

            object response;
            try
            {
                response = await method.Execute(parameters, userId, token);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                if (e is not OperationCanceledException)
                {
                    logger.LogCritical(e, "Exception thrown from {MethodName}", method.MethodName);
                }
                await WriteJsonResult(new PiwigoResponse { Stat = "fail", Error = e.HResult, Message = e.Message });
                return;
            }

            if (response is IActionResult actionResult)
            {
                await actionResult.ExecuteResultAsync(actionContext);
            }
            else if (!context.Response.HasStarted)
            {
                await WriteJsonResult(new PiwigoResponse { Result = response });
            }
        }

        private async Task WriteJsonResult(PiwigoResponse response)
        {
            var context = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new PiwigoDateTimeConverter());
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            await context.Response.CompleteAsync();
        }

        public Task ExecuteMethod(string methodName, IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token) => ExecuteMethod(GetMethod(methodName), parameters, token);
    }
}
