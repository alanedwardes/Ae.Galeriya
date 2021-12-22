using Ae.Galeriya.Core.Tables;
using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoStatusMethod : IPiwigoWebServiceMethod
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IAntiforgery _antiforgery;

        public string MethodName => "pwg.session.getStatus";
        public bool AllowAnonymous => false;

        public PiwigoStatusMethod(IHttpContextAccessor contextAccessor, IAntiforgery antiforgery)
        {
            _contextAccessor = contextAccessor;
            _antiforgery = antiforgery;
        }

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token)
        {
            return Task.FromResult<object>(new PiwigoSessionStatus
            {
                Username = _contextAccessor.HttpContext.User.Identity.Name,
                Status = "webmaster",
                Theme = "modus",
                Language = "en_GB",
                Token = _antiforgery.GetTokens(_contextAccessor.HttpContext).RequestToken,
                Charset = "utf-8",
                CurrentDatetime = DateTimeOffset.UtcNow,
                Version = "11.5.0",
                AvailableSizes = new[]
                {
                    "square",
                    "thumb",
                    "2small",
                    "xsmall",
                    "small",
                    "medium",
                    "large",
                    "xlarge",
                    "xxlarge"
                },
                UploadFileTypes = "jpg,jpeg,png,gif,mov,mp4",
                UploadFormChunkSize = 5000
            });
        }
    }
}
