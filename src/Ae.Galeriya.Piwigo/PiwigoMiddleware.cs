using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Ae.Galeriya.Piwigo
{
    public sealed class PiwigoMiddleware
    {
        private readonly RequestDelegate _next;

        public PiwigoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var dbContext = context.RequestServices.GetRequiredService<GaleriyaDbContext>();

            var repository = context.RequestServices.GetRequiredService<IPiwigoWebServiceMethodRepository>();

            var logger = context.RequestServices.GetRequiredService<ILogger<PiwigoMiddleware>>();

            if (context.Request.Path.StartsWithSegments("/blobs"))
            {
                var photoId = Path.GetFileNameWithoutExtension(context.Request.Path.ToString());

                await repository.ExecuteMethod("pwg.images.getFile", new Dictionary<string, IConvertible>
                {
                    { "image_id", photoId }
                }, context.RequestAborted);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/ws.php"))
            {
                var parameters = new Dictionary<string, IConvertible>();
                foreach (var query in context.Request.Query)
                {
                    parameters.Add(query.Key, query.Value.ToString());
                }
                if (context.Request.HasFormContentType)
                {
                    foreach (var form in context.Request.Form)
                    {
                        parameters.Add(form.Key, form.Value.ToString());
                    }
                }

                foreach (var parameter in parameters)
                {
                    logger.BeginScope(parameter.Key + ":{" + parameter.Key + "}", parameter.Value);
                }

                var requestedMethod = parameters["method"].ToString(null);
                var method = repository.GetMethod(requestedMethod);
                if (method != null)
                {
                    logger.LogInformation("Serving method {Method}", requestedMethod);
                    await repository.ExecuteMethod(method, parameters, context.RequestAborted);
                    return;
                }

                throw new NotImplementedException(requestedMethod);
            }

            await _next(context);
        }
    }
}
