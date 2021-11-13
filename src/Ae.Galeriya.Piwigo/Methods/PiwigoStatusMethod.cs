using Ae.Galeriya.Piwigo.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoStatusMethod : IPiwigoWebServiceMethod
    {
        public string MethodName => "pwg.session.getStatus";

        public Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token)
        {
            return Task.FromResult<object>(new PiwigoSessionStatus
            {
                Username = "alanedwardes",
                Status = "webmaster",
                Theme = "modus",
                Language = "en_GB",
                Token = "b13b427a677537e23206709450591cc7",
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
                UploadFormChunkSize = 500
            });
        }
    }
}
