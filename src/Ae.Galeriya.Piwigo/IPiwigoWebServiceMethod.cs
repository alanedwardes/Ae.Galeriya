using Ae.Galeriya.Core.Tables;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoWebServiceMethod
    {
        bool AllowAnonymous { get; }
        string MethodName { get; }
        Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, uint? userId, CancellationToken token);
    }
}
