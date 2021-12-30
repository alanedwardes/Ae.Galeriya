using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoWebServiceMethodRepository
    {
        IEnumerable<string> GetMethods();
        IPiwigoWebServiceMethod GetMethod(string methodName);
        Uri GetMethodUri(string methodName, IReadOnlyDictionary<string, IConvertible> parameters);
        Task ExecuteMethod(IPiwigoWebServiceMethod method, IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, CancellationToken token);
        Task ExecuteMethod(string methodName, IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, IFormFile> fileParameters, CancellationToken token);
    }
}
