using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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
            if (!context.User.Identity.IsAuthenticated && !method.AllowAnonymous)
            {
                context.Response.StatusCode = 401;
                await WriteJsonResult(new PiwigoResponse { Stat = "fail", Error = 401, Message = "Authentication required" });
                return;
            }

            RouteData routeData = context.GetRouteData();
            ActionDescriptor actionDescriptor = new ActionDescriptor();
            ActionContext actionContext = new ActionContext(context, routeData, actionDescriptor);

            var userManager = _serviceProvider.GetRequiredService<UserManager<User>>();

            User user = null;
            if (context.User.Identity.IsAuthenticated)
            {
                user = await userManager.FindByNameAsync(context.User.Identity.Name);
            }

            object response;
            try
            {
                response = await method.Execute(parameters, user, token);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
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
