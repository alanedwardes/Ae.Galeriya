using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

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

            var repository = context.RequestServices.GetRequiredService<IPiwigoWebServiceMethodRepository>();

            if (context.Request.Path.StartsWithSegments("/blobs"))
            {
                var photoId = Path.GetFileNameWithoutExtension(context.Request.Path);

                await repository.ExecuteMethod("pwg.images.getFile", new Dictionary<string, IConvertible>
                {
                    { "image_id", photoId }
                }, new Dictionary<string, FileMultipartSection>(), context.RequestAborted);
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
                }, new Dictionary<string, FileMultipartSection>(), context.RequestAborted);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/ws.php"))
            {
                var parameters = new Dictionary<string, IConvertible>();
                var files = new Dictionary<string, FileMultipartSection>();

                await ReadParameters(context, logger, parameters, files);

                var requestedMethod = parameters.GetRequired<string>("method");
                var method = repository.GetMethod(requestedMethod);
                if (method != null)
                {
                    logger.LogInformation("Serving method {Method}", requestedMethod);
                    await repository.ExecuteMethod(method, parameters, files, context.RequestAborted);
                    return;
                }

                throw new NotImplementedException(requestedMethod);
            }

            await _next(context);
        }

        private static async Task ReadParameters(HttpContext context, ILogger<PiwigoMiddleware> logger, Dictionary<string, IConvertible> parameters, Dictionary<string, FileMultipartSection> files)
        {
            foreach (var query in context.Request.Query)
            {
                parameters.Add(query.Key, query.Value.ToString());
            }

            var boundary = context.Request.GetMultipartBoundary();
            if (!string.IsNullOrEmpty(boundary))
            {
                await ReadMultipartForm(context, logger, parameters, files, boundary);
            }
            else
            {
                foreach (var form in await context.Request.ReadFormAsync(context.RequestAborted))
                {
                    parameters.Add(form.Key, form.Value.ToString());
                }
            }
        }

        private static async Task ReadMultipartForm(HttpContext context, ILogger<PiwigoMiddleware> logger, Dictionary<string, IConvertible> parameters, Dictionary<string, FileMultipartSection> files, string boundary)
        {
            logger.LogInformation("Reading multi-part sections");

            var sw = Stopwatch.StartNew();
            var reader = new MultipartReader(boundary, context.Request.Body);

            while (true)
            {
                var section = await reader.ReadNextSectionAsync(context.RequestAborted);
                if (section == null)
                {
                    break;
                }

                var disposition = section.GetContentDispositionHeader();
                if (disposition.IsFormDisposition())
                {
                    var form = section.AsFormDataSection();
                    parameters.Add(form.Name, await form.GetValueAsync());
                }

                if (disposition.IsFileDisposition())
                {
                    var file = section.AsFileSection();
                    files.Add(file.Name, file);
                }
            }

            logger.LogInformation("Finished reading multi-part sections in {TotalSeconds}", sw.Elapsed.TotalSeconds);
        }
    }
}
