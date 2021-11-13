using Ae.Galeriya.Piwigo.Entities;
using Ae.Galeriya.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class PiwigoDateTimeConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.ParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss", null);
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    public sealed class PiwigoMiddleware
    {
        private readonly RequestDelegate _next;

        public PiwigoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new PiwigoDateTimeConverter());
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            var dbContext = context.RequestServices.GetRequiredService<GalleriaDbContext>();
            await dbContext.Database.EnsureCreatedAsync();

            var repository = context.RequestServices.GetRequiredService<IPiwigoWebServiceMethodRepository>();

            var logger = context.RequestServices.GetRequiredService<ILogger<PiwigoMiddleware>>();

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

                    var response = await method.Execute(parameters, context.RequestAborted);
                    if (!context.Response.HasStarted)
                    {
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new PiwigoResponse { Result = response }, options));
                        await context.Response.CompleteAsync();
                    }
                    return;
                }

                throw new NotImplementedException(requestedMethod);
            }

            await _next(context);
        }
    }
}
