﻿using Microsoft.AspNetCore.Http;
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
            var logger = context.RequestServices.GetRequiredService<ILogger<PiwigoMiddleware>>();

            logger.LogInformation($"Middleware serving {context.Request.Method} {context.Request.Path}");

            var repository = context.RequestServices.GetRequiredService<IPiwigoWebServiceMethodRepository>();

            if (context.Request.Path.StartsWithSegments("/blobs"))
            {
                var photoId = Path.GetFileNameWithoutExtension(context.Request.Path);

                await repository.ExecuteMethod("pwg.images.getFile", new Dictionary<string, IConvertible>
                {
                    { "image_id", photoId }
                }, context.RequestAborted);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/thumbs"))
            {
                var parts = Path.GetFileNameWithoutExtension(context.Request.Path).Split('-');

                await repository.ExecuteMethod("pwg.images.getThumbnail", new Dictionary<string, IConvertible>
                {
                    { "image_id", parts[0] },
                    { "width", parts[1] },
                    { "height", parts[2] },
                    { "type", parts[3] }
                }, context.RequestAborted);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/ws.php"))
            {
                logger.LogInformation("Processing query parameters");

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

                logger.LogInformation("Processed query parameters");

                var requestedMethod = parameters.GetRequired<string>("method");
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
